using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using ISubscriber = MyNotifier.Contracts.Notifiers.ISubscriber;
//using static MyNotifier.Notifiers.FileNotifier;

namespace MyNotifier.Notifiers
{
    public abstract class Notifier : INotifier<Notifier.IPollingNotifierConnectArgs>  //polling notifier 
    {

        protected readonly IConfiguration configuration;
        protected readonly ICallContext<Notifier> callContext;

        protected bool connected = false;
        
        protected delegate void NotificationEventHandler(Notification notification); //should be void, no need to ref sender ! 
        protected event NotificationEventHandler onNotificationHandler;

        protected IPollingNotifierConnectArgs currentConnectArgs;
        protected NotificationType currentTargetNotificationTypeMask;

        protected PollTaskWrapper currentPollTaskWrapper;

        public virtual bool Connected => this.connected;

        //caching //Validate & encapsulate !!!
        //cache
        protected DateTime nextClearCache = DateTime.UtcNow;
        protected HashSet<Guid> processedNotificationIdsCache = []; //names should be unique, could override FolderObj GetHashCode() and cache full objs. using names may be lighter, but making the assumption names are unique(should be true)
        protected long lastNotificationTicks = 0L; //can use lastNotificationTicks to exclude redundant notifications and substitute for cache. however, there is possibilty of notification publication delay causing skips. maybe can only safely use cache //for now, using only cache //should work actually! 

        protected Notifier(IConfiguration configuration, ICallContext<Notifier> callContext)
        {
            this.configuration = configuration;
            this.callContext = callContext;
        }

        public virtual async ValueTask<ICallResult> ConnectAsync(IPollingNotifierConnectArgs connectArgs = null)
        {
            try
            {
                this.currentConnectArgs = connectArgs ?? this.configuration.DefaultConnectArgs;
                this.currentTargetNotificationTypeMask = connectArgs.AllowedNotificationTypeArgs.ToNotificationTypeMask();

                //check if already connected ?? //force reconnect ??
                var connectCoreResult = await this.ConnectCoreAsync().ConfigureAwait(false);
                if (!connectCoreResult.Success) return CallResult.BuildFailedCallResult(connectCoreResult, "Failed to connect");

                this.Start();

                this.connected = true;

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual async ValueTask<ICallResult> DisconnectAsync()
        {
            try
            {

                await this.currentPollTaskWrapper.KillWaitAsync(this.configuration.DisconnectTimeout).ConfigureAwait(false);

                if (this.currentPollTaskWrapper.Status == TaskStatus.Running) return new CallResult(false, "Failed to disconnect."); //encapsulate [ bool Wrapper.Wait(timespan) ]

                //var disconnectAttempts = 0;

                //this.pollTaskWrapper.TriggerCancelFlag();

                //await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);

                //while (this.pollTaskWrapper.PollTaskStillRunning)
                //{
                //    if (disconnectAttempts++ == this.configuration.DisconnectionAttemptsCount) return new CallResult(false, "Failed to disconnect.");
                //    await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);
                //}

                //if (this.pollTaskWrapper.PollTaskStatus == TaskStatus.Faulted) return this.pollTaskWrapper.BuildPollTaskFailedCallResult(); //call result may succeed but caller should verify error text  

                this.connected = false;

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected readonly int EarlyExceptionDelayMs = 2000;
        protected virtual void Start()
        {
            //this.targetNotificationTypesMask = this.configuration.AllowedNotificationTypeArgs.ToNotificationType();

            this.BeforeStartCore();

            this.currentPollTaskWrapper = this.GetPollTaskWrapper(nameof(Start));

            //register with background manager 
          
            this.currentPollTaskWrapper.Start();
        }

        protected static Exception GetPollTaskException(ICallResult getNewNotificationsResult) => new($"Failed to retrieve new notifications: {getNewNotificationsResult.ErrorText}");

        public virtual void Subscribe(ISubscriber subscriber) => this.onNotificationHandler += subscriber.OnNotification;
        public virtual void Unsubscribe(ISubscriber subscriber) => this.onNotificationHandler -= subscriber.OnNotification;
        public virtual async ValueTask OnNotificationAsync(Notification notification) => this.onNotificationHandler(notification); //make async !?! 

        protected virtual bool TryExclude(NotificationHeader notificationHeader) => this.IsExcludedNotificationType(notificationHeader) || this.IsAlreadyProcessedNotification(notificationHeader);
        protected virtual bool IsExcludedNotificationType(NotificationHeader notificationHeader) => (this.currentTargetNotificationTypeMask & notificationHeader.Type) == 0;
        protected virtual bool IsAlreadyProcessedNotification(NotificationHeader notificationHeader) => this.processedNotificationIdsCache.Contains(notificationHeader.Id);

        protected virtual void BeforeStartCore() { }

        protected abstract ValueTask<ICallResult> ConnectCoreAsync();
        protected abstract Task<ICallResult<Notification[]>> RetrieveNewNotificationsAsync();

        #region PollingTask

        protected virtual Guid pollTaskId => new("");
        protected virtual string pollTaskName => "NotifierPollTask";
        protected virtual string pollTaskDescription => "Polls for notifications";

        protected virtual PollTaskWrapper GetPollTaskWrapper(string callingFunctionName) => new(this,  new BackgroundTaskData(this.pollTaskId,
                                                                                                                              this.pollTaskName,
                                                                                                                              this.pollTaskDescription,
                                                                                                                              this.GetType().Name,
                                                                                                                              callingFunctionName));
        protected class PollTaskWrapper : BackgroundTaskWrapper
        {
            //private readonly BackgroundTaskData _data = new(new(""), "Polling notifier poll task", "Polls for notifications",
            
            private readonly Notifier notifier;

            public PollTaskWrapper(Notifier notifier, BackgroundTaskData data) : base(data) { this.notifier = notifier; }

            protected override async ValueTask ActionAsync()
            {
                try
                {
                    while (!this.killFlag.Value)
                    {
                        var getNewNotificationsResult = await this.notifier.RetrieveNewNotificationsAsync().ConfigureAwait(false);
                        if (!getNewNotificationsResult.Success)
                        {

                            //try handle notification internally

                            //otherwise, notify foreground 
                            this.OnExceptionRaised(new(getNewNotificationsResult.ErrorText)); //? appropriate to use OnException raised here? //kill task ?
                            break;
                        }

                        foreach (var notification in getNewNotificationsResult.Result) await this.notifier.OnNotificationAsync(notification).ConfigureAwait(false);

                        await Task.Delay(this.notifier.currentConnectArgs.NotificationPollingDelayMs).ConfigureAwait(false); //delay 
                    }
                }
                catch (Exception ex) { this.OnExceptionRaised(ex); } //will kill task automatically
            }
        }

        #endregion PollingTask



        public interface IPollingNotifierConnectArgs : IConnectArgs
        {
            int NotificationPollingDelayMs { get; }
        }

        public class PollingNotifierConnectArgs : ConnectArgs, IPollingNotifierConnectArgs
        {
            public int NotificationPollingDelayMs { get; set; }
        }


        #region Configuration

        public interface IConfiguration : IConfiguration<IPollingNotifierConnectArgs> 
        {
            int NotificationPollingDelayMs { get; }
        }

        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public IPollingNotifierConnectArgs DefaultConnectArgs => throw new NotImplementedException();
            public TimeSpan ClearCacheInterval => throw new NotImplementedException();
            public TimeSpan DisconnectTimeout => throw new NotImplementedException();
            public int NotificationPollingDelayMs => throw new NotImplementedException();

            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration) { }
        }

        #endregion Configuration
    }
}

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
using static MyNotifier.Notifiers.FileNotifier;

namespace MyNotifier.Notifiers
{
    public abstract class Notifier : INotifier
    {

        protected readonly IConfiguration configuration;
        protected readonly ICallContext<Notifier> callContext;

        protected bool connected = false;
        
        protected delegate ValueTask NotificationEventHandler(object sender, Notification notification);
        protected event NotificationEventHandler subscriptions;

        protected readonly NotificationType targetNotificationTypeMask;

        protected PollTaskWrapper pollTaskWrapper;

        public virtual bool Connected => this.connected;

        //caching //Validate & encapsulate !!!
        //cache
        protected DateTime nextClearCache = DateTime.UtcNow;
        protected HashSet<Guid> processedNotificationNamesCache = []; //names should be unique, could override FolderObj GetHashCode() and cache full objs. using names may be lighter, but making the assumption names are unique(should be true)
        protected long lastNotificationTicks = 0L; //can use lastNotificationTicks to exclude redundant notifications and substitute for cache. however, there is possibilty of notification publication delay causing skips. maybe can only safely use cache //for now, using only cache //should work actually! 

        public Notifier(IConfiguration configuration, ICallContext<Notifier> callContext)
        {
            this.configuration = configuration;
            this.callContext = callContext;

            this.targetNotificationTypeMask = this.configuration.AllowedNotificationTypeArgs.ToNotificationTypeMask();
        }

        public virtual async ValueTask<ICallResult> ConnectAsync(object connectArg)
        {
            try
            {
                var connectCoreResult = await this.ConnectCoreAsync(connectArg).ConfigureAwait(false);
                if (!connectCoreResult.Success) return CallResult.BuildFailedCallResult(connectCoreResult, "Failed to connect: {0}");

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
                var disconnectAttempts = 0;

                this.pollTaskWrapper.TriggerCancelFlag();

                await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);

                while (this.pollTaskWrapper.PollTaskStillRunning)
                {
                    if (disconnectAttempts++ == this.configuration.DisconnectionAttemptsCount) return new CallResult(false, "Failed to disconnect.");
                    await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);
                }

                if (this.pollTaskWrapper.PollTaskStatus == TaskStatus.Faulted) return this.pollTaskWrapper.BuildPollTaskFailedCallResult(); //call result may succeed but caller should verify error text  

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

            this.pollTaskWrapper = new();
            this.pollTaskWrapper.SetPollTask(this.PollForNotificationsAsync());

            Thread.Sleep(this.EarlyExceptionDelayMs); //to check for early exceptions, may not be necessary 

            //if exception throw before wrapping task, problem!!! check for this 
            if (this.pollTaskWrapper.PollTaskStatus == TaskStatus.Faulted) throw new Exception($"Poll task early exception: {this.pollTaskWrapper.BuildPollTaskFailedCallResult().ErrorText}");
        }

        protected static Exception GetPollTaskException(ICallResult getNewNotificationsResult) => new($"Failed to retrieve new notifications: {getNewNotificationsResult.ErrorText}");
        protected virtual async Task PollForNotificationsAsync()  //should cancelFlag & helper be given as parameters? probably would be better 
        {
            try
            {
                while (!this.pollTaskWrapper.CancelFlagValue)
                {
                    var getNewNotificationsResult = await this.RetrieveNewNotificationsAsync().ConfigureAwait(false);
                    if (!getNewNotificationsResult.Success)
                    {
                        //will propagate
                        this.pollTaskWrapper.TriggerCancelFlag(); //maybe not even necessary, i guess just for consistency's sake 
                        this.pollTaskWrapper.SetPollTaskException(GetPollTaskException(getNewNotificationsResult)); //how to handle? for now just break
                        break;
                    }

                    foreach (var notification in getNewNotificationsResult.Result) await this.OnNotificationAsync(notification).ConfigureAwait(false);

                    await Task.Delay(this.configuration.NotificationPollingDelayMs).ConfigureAwait(false); //delay 
                }
            }
            catch (Exception ex) { this.pollTaskWrapper.SetPollTaskException(ex); } //will kill task automatically 
        }

        public virtual void Subscribe(INotifier.ISubscriber subscriber) => this.subscriptions += subscriber.OnNotificationAsync;
        public virtual void Unsubscribe(INotifier.ISubscriber subscriber) => this.subscriptions -= subscriber.OnNotificationAsync;
        public virtual async ValueTask OnNotificationAsync(Notification notification) => await this.subscriptions(this, notification).ConfigureAwait(false); //make async !!! 

        protected virtual bool TryExclude(NotificationHeader notificationHeader) => this.IsExcludedNotificationType(notificationHeader) || this.IsAlreadyProcessedNotification(notificationHeader);
        protected virtual bool IsExcludedNotificationType(NotificationHeader notificationHeader) => (this.targetNotificationTypeMask & notificationHeader.Type) == 0;
        protected virtual bool IsAlreadyProcessedNotification(NotificationHeader notificationHeader) => this.processedNotificationNamesCache.Contains(notificationHeader.Id);

        protected abstract ValueTask<ICallResult> ConnectCoreAsync(object connectArg);
        protected virtual void BeforeStartCore() { }
        protected abstract Task<ICallResult<Notification[]>> RetrieveNewNotificationsAsync();

        public interface IConfiguration : IConfigurationWrapper 
        {
            AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; }
            int DisconnectionAttemptsCount { get; }
            int TryDisconnectLoopDelayMs { get; }
            int NotificationPollingDelayMs { get; }
            TimeSpan ClearCacheInterval { get; }
        }
        public class Configuration : ConfigurationWrapper, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }

            public AllowedNotificationTypeArgs AllowedNotificationTypeArgs => throw new NotImplementedException();

            public int DisconnectionAttemptsCount => throw new NotImplementedException();

            public int TryDisconnectLoopDelayMs => throw new NotImplementedException();

            public int NotificationPollingDelayMs => throw new NotImplementedException();

            public TimeSpan ClearCacheInterval => throw new NotImplementedException();
        }


        protected class PollTaskWrapper
        {

            private BooleanFlag cancelFlag = new() { Value = false };
            private Task pollTask;
            private Exception pollTaskException;

            private readonly SemaphoreSlim semaphore = new(1, 1);

            public bool CancelFlagValue => this.cancelFlag.Value;
            public TaskStatus PollTaskStatus => this.pollTask.Status;
            public bool PollTaskStillRunning => this.pollTask.Status != TaskStatus.RanToCompletion && this.pollTask.Status != TaskStatus.Faulted && this.pollTask.Status != TaskStatus.Canceled;

            public ICallResult BuildPollTaskFailedCallResult() => new CallResult(true, $"Poll task faulted: {this.pollTaskException.Message}");

            public void SetPollTask(Task pollTask)
            {
                try
                {
                    this.semaphore.Wait();
                    this.pollTask = pollTask;
                }
                finally { this.semaphore.Release(); }
            }

            public void TriggerCancelFlag()
            {
                try
                {
                    this.semaphore.Wait();
                    this.cancelFlag.Value = true;
                }
                finally { this.semaphore.Release(); }
            }

            public void SetPollTaskException(Exception ex)
            {
                try
                {
                    this.semaphore.Wait();
                    this.pollTaskException = ex;
                }
                finally { this.semaphore.Release(); }
            }
        }
    }

    public class AllowedNotificationTypeArgs
    {
        public bool Updates { get; set; }
        public bool Commands { get; set; }
        public bool Exceptions { get; set; }

        public NotificationType ToNotificationTypeMask()
        {
            var ret = new NotificationType();

            if (this.Updates) ret += (byte)NotificationType.Update;
            if (this.Commands) ret += ((byte)NotificationType.Command + (byte)NotificationType.CommandResult); //take this.Commands=true to also permit commandResult types 
            if (this.Exceptions) ret += (byte)NotificationType.Exception;

            return ret;
        }
    }
}

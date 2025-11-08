//using MyNotifier.Base;
//using MyNotifier.Contracts.Base;
//using MyNotifier.Contracts.FileIOManager;
//using MyNotifier.Contracts.Notifications;
//using MyNotifier.Contracts.Notifiers;
//using MyNotifier.Contracts.Proxy;
//using MyNotifier.FileIOManager;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace MyNotifier.Notifiers
//{
//    public partial class FileNotifier : Notifier, IFileNotifier
//    {

//        private readonly IFileIOManager fileIOManager;
//        private readonly INotificationFileSystemObjectTranslator translator;
//        private readonly IConfiguration configuration;
//        private readonly ICallContext<FileNotifier> callContext;

//        private FileNotifierHelper helper;

//        //private NotificationType targetNotificationTypesMask;

//        private PollTaskWrapper pollTaskWrapper;


//        public FileNotifier(IFileIOManager fileIOManager,
//                            INotificationFileSystemObjectTranslator translator,
//                            IConfiguration configuration,
//                            ICallContext<FileNotifier> callContext) : base(configuration, callContext) 
//        {
//            this.fileIOManager = fileIOManager;
//            this.translator = translator;
//            this.configuration = configuration;
//            this.callContext = callContext; 
//        }

//        public override async ValueTask<ICallResult> ConnectAsync(object connectArg) //bool reset connection ? //force reconnect 
//        {
//            try
//            {
//                var directoryExistsResult = await this.fileIOManager.DirectoryExistsAsync(this.configuration.NotificationsDirectoryName).ConfigureAwait(false); //could configure fileIOManager with notifications directory name 

//                if (!directoryExistsResult.Success) return CallResult.BuildFailedCallResult(directoryExistsResult, "Failed to connect: {0}");
//                if (!directoryExistsResult.Result) return new CallResult(false, $"Notification directory {this.configuration.NotificationsDirectoryName} does not exist");

//                this.Start();

//                this.connected = true;

//                return new CallResult();
//            }
//            catch(Exception ex) { return CallResult.FromException(ex); }
//        }

//        public override async ValueTask<ICallResult> DisconnectAsync()
//        {
//            try
//            {
//                var disconnectAttempts = 0;

//                this.pollTaskWrapper.TriggerCancelFlag();

//                await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);

//                while (this.pollTaskWrapper.PollTaskStillRunning)
//                {
//                    if (disconnectAttempts++ == this.configuration.DisconnectionAttemptsCount) return new CallResult(false, "Failed to disconnect.");
//                    await Task.Delay(this.configuration.TryDisconnectLoopDelayMs).ConfigureAwait(false);
//                }

//                if (this.pollTaskWrapper.PollTaskStatus == TaskStatus.Faulted) return this.pollTaskWrapper.BuildPollTaskFailedCallResult(); //call result may succeed but caller should verify error text  

//                this.connected = false;

//                return new CallResult();
//            }
//            catch (Exception ex) { return CallResult.FromException(ex); }
//        }

//        protected override ValueTask<ICallResult> ConnectCoreAsync(object connectArg)
//        {
//            throw new NotImplementedException();
//        }

//        protected override Task<ICallResult<Notification[]>> RetrieveNewNotificationsAsync()
//        {
//            throw new NotImplementedException();
//        }


//        private readonly int EarlyExceptionDelayMs = 2000;
//        private void Start()
//        {
//            //this.targetNotificationTypesMask = this.configuration.AllowedNotificationTypeArgs.ToNotificationType();

//            this.helper = new FileNotifierHelper(this.fileIOManager, this.translator, this.configuration);

//            this.pollTaskWrapper = new();
//            this.pollTaskWrapper.SetPollTask(this.PollForNotificationsAsync());

//            Thread.Sleep(this.EarlyExceptionDelayMs); //to check for early exceptions, may not be necessary 

//            //if exception throw before wrapping task, problem!!! check for this 
//            if (this.pollTaskWrapper.PollTaskStatus == TaskStatus.Faulted) throw new Exception($"Poll task early exception: {this.pollTaskWrapper.BuildPollTaskFailedCallResult().ErrorText}");
//        }

//        private async Task PollForNotificationsAsync()  //should cancelFlag & helper be given as parameters? probably would be better 
//        {
//            try
//            {
//                while (!this.pollTaskWrapper.CancelFlagValue)
//                {
//                    var getNewNotificationsResult = await this.helper.RetrieveNewNotificationsAsync().ConfigureAwait(false);
//                    if (!getNewNotificationsResult.Success)
//                    {
//                        //will propagate
//                        this.pollTaskWrapper.TriggerCancelFlag(); //maybe not even necessary, i guess just for consistency's sake 
//                        this.pollTaskWrapper.SetPollTaskException(new Exception($"Failed to retrieve new notifications: {getNewNotificationsResult.ErrorText}")); //how to handle? for now just break
//                        break;
//                    }

//                    foreach (var notification in getNewNotificationsResult.Result) await this.OnNotificationAsync(notification).ConfigureAwait(false);

//                    await Task.Delay(this.configuration.NotificationPollingDelayMs).ConfigureAwait(false); //delay 
//                }
//            } catch(Exception ex) { this.pollTaskWrapper.SetPollTaskException(ex); } //will kill task automatically 
//        }
//    }

//    public interface IFileNotifier : INotifier { }
//}

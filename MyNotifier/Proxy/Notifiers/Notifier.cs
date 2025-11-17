using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IIOManager = MyNotifier.Contracts.Proxy.Notifiers.IIOManager;

namespace MyNotifier.Proxy.Notifiers
{
    public class Notifier : MyNotifier.Notifiers.Notifier
    {

        private readonly IIOManager ioManager;

        public Notifier(IIOManager ioManager, 
                        IConfiguration configuration, 
                        ICallContext<Notifier> callContext) : base(configuration, callContext) { this.ioManager = ioManager; }

        protected override async ValueTask<ICallResult> ConnectCoreAsync()
        {
            try
            {
                var notificationDirectoryExists = await this.ioManager.NotificationDirectoryExistsAsync().ConfigureAwait(false); //Assert? //Ensure? //Make configureable which one ? 

                if (!notificationDirectoryExists.Success) return CallResult.BuildFailedCallResult(notificationDirectoryExists, "Failed to validate notifications directory exists");
                if (!notificationDirectoryExists.Result) return new CallResult(false, "Notifications Directory does not exist.");

                return new CallResult();
            } catch(Exception ex) { return CallResult.FromException(ex); }
        }

        protected override async Task<ICallResult<Notification[]>> RetrieveNewNotificationsAsync()
        {
            try
            {
                var retrieveNotificationHeadersResult = await this.ioManager.RetrieveNotificationHeadersAsync().ConfigureAwait(false);
                if (!retrieveNotificationHeadersResult.Success) return CallResult<Notification[]>.BuildFailedCallResult(retrieveNotificationHeadersResult, "Failed to retrieve notifications"); //cast to be safe ? 

                var notificationHeaders = retrieveNotificationHeadersResult.Result;
                var latestTicks = this.lastNotificationTicks;

                var notifications = new Notification[notificationHeaders.Length];

                for (int i = 0; i < notifications.Length; i++)
                {
                    var notificationHeader = notificationHeaders[i];

                    if (this.TryExclude(notificationHeader)) continue;

                    var ReadInNotificationResult = await this.ioManager.ReadInNotificationAsync(notificationHeader).ConfigureAwait(false);
                    //how to handle? for now fail all. !!!
                    if (!ReadInNotificationResult.Success) return CallResult<Notification[]>.BuildFailedCallResult(ReadInNotificationResult, $"Failed to read in notification with Id:{notificationHeader.Id}");

                    notifications[i] = ReadInNotificationResult.Result;

                    if (notificationHeader.Ticks > latestTicks) latestTicks = notificationHeader.Ticks;

                    var nowTime = DateTime.UtcNow;
                    if (this.nextClearCache > nowTime)
                    {
                        this.processedNotificationIdsCache.Clear();
                        this.nextClearCache = nowTime + this.configuration.ClearCacheInterval;
                    }

                    this.processedNotificationIdsCache.Add(notificationHeader.Id);
                }

                this.lastNotificationTicks = latestTicks;

                return new CallResult<Notification[]>(notifications);
            }
            catch (Exception ex) { return CallResult<Notification[]>.FromException(ex); }
        }
    }
}

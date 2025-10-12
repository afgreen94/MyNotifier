using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Base;

namespace MyNotifier.Notifiers
{
    public partial class FileNotifier : Notifier, IFileNotifier
    {
        public new interface IConfiguration : Notifier.IConfiguration
        {
            string NotificationsDirectoryName { get; } //path?
            string MetadataFileName { get; }
            string DataFileName { get; }
            AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; }
            int DisconnectionAttemptsCount { get; }
            int TryDisconnectLoopDelayMs { get; }
            int NotificationPollingDelayMs { get; }
            bool DeleteNotificationOnDelivered { get; }
            WriteCompleteSignalArgs WriteCompleteSignalArgs { get; }
            TimeSpan ClearCacheInterval { get; }

        }
        public new class Configuration : Notifier.Configuration, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }

            public string NotificationsDirectoryName { get; set; }
            public string MetadataFileName { get; set; }
            public string DataFileName { get; set; }
            public AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; set; }
            public int NotificationPollingDelayMs { get; set; }
            public WriteCompleteSignalArgs WriteCompleteSignalArgs { get; set; }
            public bool DeleteNotificationOnDelivered { get; set; }
            public int DisconnectionAttemptsCount { get; set; }
            public int TryDisconnectLoopDelayMs { get; set; }
            public TimeSpan ClearCacheInterval { get; set; }
        }

        // only scheme for now, could be configurable later
        public class WriteCompleteSignalArgs //default values by convention should generally be false 
        {
            public string Name { get; set; } = "write_complete";
            public int[] RetrySequenceDelaysMs { get; set; } = [5000, 10000, 30000, 60000]; //not to worry about bottlenecking performance, since running full sequence should really only occur during actual exceptions. will handle oversized file cases later
            public bool DeleteWriteCompleteFileOnDelivered { get; set; } = true;
        }
    }
}

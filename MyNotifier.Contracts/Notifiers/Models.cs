using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Notifiers
{
    public class AllowedNotificationTypeArgs
    {
        public bool Updates { get; set; }
        public bool Commands { get; set; }
        public bool CommandResults { get; set; }
        public bool Exceptions { get; set; }

        public NotificationType ToNotificationTypeMask()
        {
            var ret = new NotificationType();

            if (this.Updates) ret += (byte)NotificationType.Update;
            if (this.Commands) ret += (byte)NotificationType.Command;
            if (this.CommandResults) ret += (byte)NotificationType.CommandResult;
            if (this.Exceptions) ret += (byte)NotificationType.Exception;

            return ret;
        }
    }
    public class ConnectArgs : IConnectArgs
    {
        public AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; set; }
    }
}

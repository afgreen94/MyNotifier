using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy.Publishers
{
    public interface IIOManager
    {
        ValueTask<ICallResult> WriteNotificationFilesAsync(Notification notification);
    }
}

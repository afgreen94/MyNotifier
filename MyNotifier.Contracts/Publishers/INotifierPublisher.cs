using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Publishers
{
    public interface INotifierPublisher : IInitializeable
    {
        ValueTask<ICallResult> PublishAsync(Notification notification);
    }
}

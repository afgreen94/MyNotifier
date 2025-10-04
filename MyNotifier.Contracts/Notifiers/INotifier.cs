using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Contracts.Notifiers
{
    public interface INotifier
    {
        bool Connected { get; }
        ValueTask<ICallResult> ConnectAsync(object connectArg);
        ValueTask<ICallResult> DisconnectAsync();
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriper);

        public interface ISubscriber
        {
            Definition Definition { get; }
            ValueTask OnNotificationAsync(object sender, Notification notification);
        }
    }

    public interface INotifierArgs { object FactoryArgs { get; } }

}

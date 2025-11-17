using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Contracts.Notifiers
{
    public interface INotifier<TConnectArgs>
        where TConnectArgs : class, IConnectArgs
    {
        bool Connected { get; }
        ValueTask<ICallResult> ConnectAsync(TConnectArgs connectArgs = null);
        ValueTask<ICallResult> DisconnectAsync();
        void Subscribe(ISubscriber subscriber);
        void Unsubscribe(ISubscriber subscriper);
    }

    public interface INotifier : INotifier<IConnectArgs> { }

    public interface ISubscriber
    {
        Definition Definition { get; }
        void OnNotification(Notification notification);  //ICallResult ?
    }

    public interface IConnectArgs
    {
        AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; }
    }

    public interface IConfiguration<TConnectArgs> : IApplicationConfigurationWrapper
        where TConnectArgs : IConnectArgs
    {
        TConnectArgs DefaultConnectArgs { get; }
        TimeSpan ClearCacheInterval { get; }
        TimeSpan DisconnectTimeout { get; }
    }

    public interface INotifierArgs { object FactoryArgs { get; } }
}

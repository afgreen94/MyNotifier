using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Notifiers
{
    public abstract class Notifier : INotifier
    {

        protected readonly IConfiguration configuration;
        protected readonly ICallContext<Notifier> callContext;

        protected bool connected = false;
        
        protected delegate void NotificationEventHandler(object sender, Notification notification);
        protected event NotificationEventHandler subscriptions;

        public virtual bool Connected => this.connected;

        public Notifier(IConfiguration configuration, ICallContext<Notifier> callContext)
        {
            this.configuration = configuration;
            this.callContext = callContext;
        }

        public abstract ValueTask<ICallResult> ConnectAsync(object connectArg);
        public abstract ValueTask<ICallResult> DisconnectAsync();

        public virtual void Subscribe(INotifier.ISubscriber subscriber) => this.subscriptions += subscriber.OnNotification;
        public virtual void Unsubscribe(INotifier.ISubscriber subscriber) => this.subscriptions -= subscriber.OnNotification;
        public virtual void OnNotification(Notification notification) => this.subscriptions(this, notification);

        public interface IConfiguration : IConfigurationWrapper { }
        public class Configuration : ConfigurationWrapper, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }
        }
    }
}

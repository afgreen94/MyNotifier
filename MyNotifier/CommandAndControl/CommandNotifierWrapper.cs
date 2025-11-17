using MyNotifier.Base;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Notifiers;
using ISubscriber = MyNotifier.Contracts.Notifiers.ISubscriber;
using static MyNotifier.ApplicationForeground;

namespace MyNotifier.CommandAndControl
{
    public class CommandNotifierWrapper : ICommandNotifierWrapper  //enforce only COMMAND notification type 
    {
        private readonly IPollingNotifier innerNotifier;

        private Subscriber _subscriber;
        private Subscriber subscriber => this._subscriber ??= new Subscriber(this);

        private delegate void OnCommandHandler(CommandArgs args);
        private event OnCommandHandler onCommandHandler;

        public bool Connected => this.innerNotifier.Connected;

        public CommandNotifierWrapper(IPollingNotifier innerNotifier) { this.innerNotifier = innerNotifier; }

        public async ValueTask<ICallResult> ConnectAsync()
        {
            try
            {
                this.innerNotifier.Subscribe(this.subscriber);

                var connectArgs = new Notifier.PollingNotifierConnectArgs() { AllowedNotificationTypeArgs = AllowedNotificationTypeArgs.FromNotificationTypeMask(NotificationType.Command) };

                return await this.innerNotifier.ConnectAsync(connectArgs).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }
        public async ValueTask<ICallResult> DisconnectAsync()
        {
            try
            {
                this.innerNotifier.Unsubscribe(this.subscriber);

                return await this.innerNotifier.DisconnectAsync();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public void RegisterCommandSubscriber(ICommandSubscriber subscriber) => this.onCommandHandler += subscriber.OnCommand;


        protected void OnNotification(Notification notification) //events should return call results ?
        {
            //parse command 
            //trigger onCommand 
        }


        public class Subscriber : Contracts.Notifiers.ISubscriber
        {
            private readonly Definition definition = new()
            {
                Id = Guid.NewGuid(),
                Name = "CommandNotifierWrapper.NotifierSubscriber",
                Description = "Handles command notifications from the CommandNotifierWrapper.",
            };

            private readonly CommandNotifierWrapper commandNotifierWrapper;

            public Definition Definition => this.definition;

            public Subscriber(CommandNotifierWrapper commandNotifierWrapper) { this.commandNotifierWrapper = commandNotifierWrapper; }

            public void OnNotification(Notification notification) => this.commandNotifierWrapper.OnNotification(notification);
        }
    }


    public interface ICommandNotifierWrapper
    {
        bool Connected { get; }

        ValueTask<ICallResult> ConnectAsync();
        ValueTask<ICallResult> DisconnectAsync();

        void RegisterCommandSubscriber(ICommandSubscriber subscriber);
        //void Unregister(Guid subscriberId);
    }
}

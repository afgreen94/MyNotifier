using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    public interface ICommandAndController  //yes the name is stupid //use entirely separate (standalone) notifier ?
    {
        ValueTask<ICallResult> InitializeAsync(bool forceReinitialize);
        void Register(IControllable controllable);
        void Unregister(IControllable controllable);
    }

    public class CommandAndController : ICommandAndController
    {
        private readonly INotifierPublisher publisher;
        private readonly INotifier notifier;
        private readonly ICallContext<CommandAndController> callContext;

        private readonly NotificationBuilder notificationBuilder = new();

        //eventually may have more than one, for now one controllable per command 
        //private readonly IDictionary<Type, IControllable> typeToControllableMap = new Dictionary<Type, IControllable>();
        private readonly IDictionary<Guid, IControllable> commandDefinitionIdToControllableMap = new Dictionary<Guid, IControllable>();
        //private readonly IDictionary<Guid, HashSet<IControllable>> commandDefinitionIdToControllableSetMap = new Dictionary<Guid, HashSet<IControllable>>();

        private bool isInitialized = false;

        public CommandAndController(INotifierPublisher publisher, INotifier notifier, ICallContext<CommandAndController> callContext)
        {
            this.publisher = publisher;
            this.notifier = notifier;
            this.callContext = callContext;
        }

        //public ValueTask<ICallResult> ConnectAsync(INotifier notifier)
        //{

        //}

        public async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false)
        {
            try
            {
                if (!this.isInitialized || forceReinitialize)
                {

                    //subscribe to notifier with COMMANDS target-type mask 
                    this.notifier.Subscribe(new NotifierSubscriber(this));

                    //connect notifier if not connected //is this a good idea ? 
                    bool forceConnectNotifier = false;
                    if (forceConnectNotifier && !this.notifier.Connected) 
                    {
                        var connectResult = await this.notifier.ConnectAsync(null).ConfigureAwait(false);
                        if (!connectResult.Success) return CallResult.BuildFailedCallResult(connectResult, "Failed to connect to command notifier: {0}");
                    }

                    this.isInitialized = true;
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected ICommand ParseCommand(Notification notification) => throw new NotImplementedException();
        protected async ValueTask OnCommandAsync(ICommand command)
        {
            if (this.commandDefinitionIdToControllableMap.TryGetValue(command.Definition.Id, out IControllable controllable))
            {
                var result = await controllable.OnCommandAsync(command).ConfigureAwait(false);

                //if command fails? 

                bool publishCommandResult = false; //if should publish result 
                if (publishCommandResult)
                {
                    var notification = this.notificationBuilder.Build(result);
                    var publishResult = await this.publisher.PublishAsync(notification).ConfigureAwait(false);
                }
            }
        }

        public void Register(IControllable controllable)
        {
            throw new NotImplementedException();
        }

        public void Unregister(IControllable controllable)
        {
            throw new NotImplementedException();
        }


        public class NotifierSubscriber : INotifier.ISubscriber
        {
            private readonly CommandAndController commandReceiever;

            public Definition Definition => throw new NotImplementedException();

            public NotifierSubscriber(CommandAndController commandReceiever) { this.commandReceiever = commandReceiever; }

            public async ValueTask OnNotificationAsync(object sender, Notification notification)
            {
                var command = this.commandReceiever.ParseCommand(notification);
                await this.commandReceiever.OnCommandAsync(command).ConfigureAwait(false);
            }
        }
    }
}

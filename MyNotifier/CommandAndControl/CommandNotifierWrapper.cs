using MyNotifier.Base;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICommand = MyNotifier.Contracts.CommandAndControl.Commands.ICommand;

namespace MyNotifier.CommandAndControl
{
    public class CommandNotifierWrapper : INotifier
    {

        private readonly INotifier notifier;

        private readonly HashSet<ExpectedCommandResultToken> expectedCommandResults = [];

        public bool Connected => notifier.Connected;

        public CommandNotifierWrapper(INotifier notifier) { this.notifier = notifier; }

        public ValueTask<ICallResult> ConnectAsync(object connectArg) => notifier.ConnectAsync(connectArg);
        public ValueTask<ICallResult> DisconnectAsync() => notifier.DisconnectAsync();
        public void Subscribe(INotifier.ISubscriber subscriber) => notifier.Subscribe(subscriber);
        public void Unsubscribe(INotifier.ISubscriber subscriper) => notifier.Unsubscribe(subscriper);


        public void RegisterExpectedCommandResult(ExpectedCommandResultToken ecrToken) => expectedCommandResults.Add(ecrToken);
        private void CheckExpiries() { }
    }


    public class CommandObject
    {
        private readonly CommandNotifierWrapper commandNotifier;
        private readonly IConfiguration configuration;
        private readonly ICallContext<CommandObject> callContext;

        //private CommandNotifierSubscriber subscriber; //may or may not need subscriber reference
        private IDictionary<Guid, IControllable> controllables = new Dictionary<Guid, IControllable>();

        private bool isInitialized = false;

        public CommandObject(CommandNotifierWrapper commandNotifier, 
                             IConfiguration configuration, 
                             ICallContext<CommandObject> callContext)
        {
            this.commandNotifier = commandNotifier;
            this.configuration = configuration;
            this.callContext = callContext;
        }


        public async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize)
        {
            try
            {
                if(!isInitialized || forceReinitialize)
                {

                    //subscribe to notifier with COMMANDS target-type mask 
                    commandNotifier.Subscribe(new CommandNotifierSubscriber(this));

                    //connect notifier if not connected 
                    if (!commandNotifier.Connected)
                    {
                        var connectResult = await commandNotifier.ConnectAsync(null).ConfigureAwait(false);
                        if (!connectResult.Success) return CallResult.BuildFailedCallResult(connectResult, "Failed to connect to command notifier: {0}");
                    }

                    isInitialized = true;
                }

                return new CallResult();

            } catch(Exception ex) { return CallResult.FromException(ex); }
        }


        public void RegisterControllable(IControllable controllable, bool allowOverwrite = false) //may pass in controllables from ctor 
        {
            if (controllables.ContainsKey(controllable.Definition.Id) && !allowOverwrite) throw new Exception("Cannot overwrite existing controllable.");

            controllables[controllable.Definition.Id] = controllable;
        }


        public void RegisterControllable<TCommand>(IControllable<TCommand> controllable, bool allowOverwrite = false)
            where TCommand : ICommand
        {

        }


        protected async ValueTask OnCommandAsync(INotifier sender, Notification notification)
        {
            //parse command type 
            var command = ParseCommand(notification);
            //adjust appropriate controllables 
            AdjustControllable(command);
        }


        private ICommand ParseCommand(Notification notification) => throw new NotImplementedException();
        private void AdjustControllable(ICommand command) { }


        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }

        public interface IControllable 
        {
            IDefinition Definition { get; }    
            public interface IDefinition : Contracts.Base.IDefinition { }
        }


        public interface IControllable<TCommand> : IControllable
            where TCommand : ICommand
        {
            void OnCommand(TCommand command);   
        }

        public abstract class Controllable<TCommand> : IControllable
            where TCommand : ICommand
        {
            public IControllable.IDefinition Definition => throw new NotImplementedException();

            public virtual void OnCommand(ICommand command)
            {
                //validate 

                if (command is not TCommand coreCommand) throw new Exception("I always forget if C# allows narrowing casts. probably not explicityly tbh.");

                OnCommandCore(coreCommand); //does C# allow narrowing casts? I always forget this...
            }

            protected abstract void OnCommandCore(TCommand command);
        }


        protected class CommandNotifierSubscriber : INotifier.ISubscriber
        {
            private readonly Definition definition = new()
            {
                Id = new Guid("{}"),
                Name = "",
                Description = ""
            };

            private readonly CommandObject commandObject;

            public CommandNotifierSubscriber(CommandObject commandObject) { this.commandObject = commandObject; }

            public Definition Definition => definition;

            public ValueTask OnNotificationAsync(object sender, Notification notification) => commandObject.OnCommandAsync((INotifier)sender, notification);
        }
    }


    public interface IControllable<TCommand>
    where TCommand : ICommand
    {
        void OnCommand(TCommand command);
    }


    public class Backgrounder 
    {
        public async Task WaitAsync() { }
    }




    public class ExpectedCommandResultToken
    {
        public Guid CommandId { get; set; }

        public DateTime PublishedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public OnResultReceivedArgs OnResultReceivedArgs { get; set; }
        public OnExpiryArgs OnExpiryArgs { get; set; }
    }

    public class OnExpiryArgs { }
    public class OnResultReceivedArgs { }
}

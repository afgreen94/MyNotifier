using MyNotifier.Base;
using MyNotifier.Commands;
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
using ICommand = MyNotifier.Contracts.Commands.ICommand;
using MyNotifier.Contracts.Commands;

namespace MyNotifier.Notifiers
{
    public class CommandNotifierWrapper : INotifier
    {

        private readonly INotifier notifier;

        private readonly HashSet<ExpectedCommandResultToken> expectedCommandResults = new();

        public bool Connected => this.notifier.Connected;

        public CommandNotifierWrapper(INotifier notifier) { this.notifier = notifier; }

        public ValueTask<ICallResult> ConnectAsync(object connectArg) => this.notifier.ConnectAsync(connectArg);
        public ValueTask<ICallResult> DisconnectAsync() => this.notifier.DisconnectAsync();
        public void Subscribe(INotifier.ISubscriber subscriber) => this.notifier.Subscribe(subscriber);
        public void Unsubscribe(INotifier.ISubscriber subscriper) => this.notifier.Unsubscribe(subscriper);


        public void RegisterExpectedCommandResult(ExpectedCommandResultToken ecrToken) => this.expectedCommandResults.Add(ecrToken);
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
                if(!this.isInitialized || forceReinitialize)
                {

                    //subscribe to notifier with COMMANDS target-type mask 
                    this.commandNotifier.Subscribe(new CommandNotifierSubscriber(this));

                    //connect notifier if not connected 
                    if (!this.commandNotifier.Connected)
                    {
                        var connectResult = await this.commandNotifier.ConnectAsync(null).ConfigureAwait(false);
                        if (!connectResult.Success) return CallResult.BuildFailedCallResult(connectResult, "Failed to connect to command notifier: {0}");
                    }

                    this.isInitialized = true;
                }

                return new CallResult();

            } catch(Exception ex) { return CallResult.FromException(ex); }
        }


        public void RegisterControllable(IControllable controllable, bool allowOverwrite = false) //may pass in controllables from ctor 
        {
            if (this.controllables.ContainsKey(controllable.Definition.Id) && !allowOverwrite) throw new Exception("Cannot overwrite existing controllable.");

            this.controllables[controllable.Definition.Id] = controllable;
        }


        public void RegisterControllable<TCommand>(IControllable<TCommand> controllable, bool allowOverwrite = false)
            where TCommand : ICommand
        {

        }


        protected void OnCommand(INotifier sender, Notification notification)
        {
            //parse command type 
            var command = this.ParseCommand(notification);
            //adjust appropriate controllables 
            this.AdjustControllable(command);
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
            where TCommand : MyNotifier.Contracts.Commands.ICommand
        {
            public IControllable.IDefinition Definition => throw new NotImplementedException();

            public virtual void OnCommand(ICommand command)
            {
                //validate 

                if (command is not TCommand coreCommand) throw new Exception("I always forget if C# allows narrowing casts. probably not explicityly tbh.");

                this.OnCommandCore(coreCommand); //does C# allow narrowing casts? I always forget this...
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

            public void OnNotification(object sender, Notification notification) => this.commandObject.OnCommand((INotifier)sender, notification);
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

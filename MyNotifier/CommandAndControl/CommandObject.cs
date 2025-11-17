using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    //WIP for possible future command layer

    //public class CommandObject  //receives and executes commands on registered IControllables 
    //{
    //    private readonly CommandNotifierWrapper commandNotifier;
    //    private readonly IConfiguration configuration;
    //    private readonly ICallContext<CommandObject> callContext;

    //    //private CommandNotifierSubscriber subscriber; //may or may not need subscriber reference
    //    private IDictionary<Guid, IControllable> controllables = new Dictionary<Guid, IControllable>();

    //    private bool isInitialized = false;

    //    public CommandObject(CommandNotifierWrapper commandNotifier,
    //                         IConfiguration configuration,
    //                         ICallContext<CommandObject> callContext)
    //    {
    //        this.commandNotifier = commandNotifier;
    //        this.configuration = configuration;
    //        this.callContext = callContext;
    //    }


    //    public async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize)
    //    {
    //        try
    //        {
    //            if (!this.isInitialized || forceReinitialize)
    //            {

    //                //subscribe to notifier with COMMANDS target-type mask 
    //                this.commandNotifier.RegisterCommandSubscriber(null); //!!!

    //                //connect notifier if not connected 
    //                if (!this.commandNotifier.Connected)
    //                {
    //                    var connectResult = await this.commandNotifier.ConnectAsync().ConfigureAwait(false);
    //                    if (!connectResult.Success) return CallResult.BuildFailedCallResult(connectResult, "Failed to connect to command notifier");
    //                }

    //                this.isInitialized = true;
    //            }

    //            return new CallResult();

    //        }
    //        catch (Exception ex) { return CallResult.FromException(ex); }
    //    }


    //    public void RegisterControllable(IControllable controllable, bool allowOverwrite = false) //may pass in controllables from ctor 
    //    {
    //        if (this.controllables.ContainsKey(controllable.Definition.Id) && !allowOverwrite) throw new Exception("Cannot overwrite existing controllable.");

    //        this.controllables[controllable.Definition.Id] = controllable;
    //    }


    //    private IDictionary<Guid, HashSet<IControllable<ICommand>>> map = new Dictionary<Guid, HashSet<IControllable<ICommand>>>();
    //    public void RegisterControllable<TCommand>(IControllable<TCommand> controllable, bool allowOverwrite = false)
    //        where TCommand : ICommand
    //    {
    //        //maybe map to command type instead of just id 



    //    }


    //    protected async ValueTask OnCommandAsync(INotifier sender, Notification notification)
    //    {
    //        //parse command type 
    //        var command = this.ParseCommand(notification);

    //        //adjust appropriate controllables 
    //        await this.AdjustControllableAsync(command).ConfigureAwait(false);
    //    }


    //    private ICommand ParseCommand(Notification notification) => throw new NotImplementedException();
    //    private async ValueTask AdjustControllableAsync(ICommand command) => throw new NotImplementedException();


    //    public interface IConfiguration : IApplicationConfigurationWrapper { }
    //    public class Configuration : ApplicationConfigurationWrapper, IConfiguration
    //    {
    //        public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
    //        {
    //        }
    //    }


    //    protected class CommandNotifierSubscriber : Contracts.Notifiers.ISubscriber
    //    {
    //        private readonly Definition definition = new()
    //        {
    //            Id = new Guid("{}"),
    //            Name = "",
    //            Description = ""
    //        };

    //        private readonly CommandObject commandObject;

    //        public CommandNotifierSubscriber(CommandObject commandObject) { this.commandObject = commandObject; }

    //        public Definition Definition => this.definition;

    //        public void OnNotification(Notification notification)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public async ValueTask OnNotificationAsync(object sender, Notification notification) => await this.commandObject.OnCommandAsync((INotifier)sender, notification).ConfigureAwait(false);
    //    }

    //public void RegisterExpectedCommandResult(ExpectedCommandResultToken ecrToken) => this.expectedCommandResults.Add(ecrToken);
    //private void CheckExpiries() { }

    //}


    //public class ExpectedCommandResultToken
    //{
    //    public Guid CommandId { get; set; }

    //    public DateTime PublishedAt { get; set; }
    //    public DateTime ExpiresAt { get; set; }

    //    public OnResultReceivedArgs OnResultReceivedArgs { get; set; }
    //    public OnExpiryArgs OnExpiryArgs { get; set; }
    //}

    //public class OnExpiryArgs { }
    //public class OnResultReceivedArgs { }
}

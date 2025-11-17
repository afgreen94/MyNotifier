using MyNotifier.Base;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts;
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
using static MyNotifier.ApplicationForeground;

namespace MyNotifier.CommandAndControl
{

    
    public class CommandNotifierWrapper0
    {
        private readonly INotifier innerNotifier;

        public bool Connected => this.innerNotifier.Connected;

        public ValueTask<ICallResult> ConnectAsync() => this.innerNotifier.ConnectAsync();
        public ValueTask<ICallResult> DisconnectAsync() => this.innerNotifier.DisconnectAsync();

    }


    public class CommandNotifierWrapper : ICommandNotifierWrapper  //enforce only COMMAND notification type 
    {

        private readonly INotifier innerNotifier;

        //private readonly HashSet<ExpectedCommandResultToken> expectedCommandResults = [];

        public bool Connected => this.innerNotifier.Connected;

        public CommandNotifierWrapper(INotifier innerNotifier) { this.innerNotifier = innerNotifier; }

        public ValueTask<ICallResult> ConnectAsync() => this.innerNotifier.ConnectAsync();
        public ValueTask<ICallResult> DisconnectAsync() => this.innerNotifier.DisconnectAsync();

        public void RegisterCommandSubscriber(ICommandSubscriber subscriber)
        {
            throw new NotImplementedException();
        }


        //public void RegisterExpectedCommandResult(ExpectedCommandResultToken ecrToken) => this.expectedCommandResults.Add(ecrToken);
        //private void CheckExpiries() { }
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

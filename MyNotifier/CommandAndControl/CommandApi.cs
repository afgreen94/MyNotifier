using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;

namespace MyNotifier.CommandAndControl
{
    public abstract class CommandApi : ICommandApi  
    {
        private readonly ICommandIssue commandIssue;
        private readonly ICallContext<CommandApi> callContext;

        public CommandApi(ICommandIssue commandIssue, ICallContext<CommandApi> callContext) { this.commandIssue = commandIssue; this.callContext = callContext; }

        public Task<ICallResult> ChangeApplicationConfigurationAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> ChangeApplicationConfigurationAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> RegisterAndSubscribeToNewInterestsAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> RegisterAndSubscribeToNewInterestsAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> SubscribeToInterestsByIdAsync(Guid[] interestIds)
        {
            throw new NotImplementedException();
        }
        public Task<ICommandResult> SubscribeToInterestsByIdAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> SubscribeToInterestsByIdAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> UnsubscribeFromInterestsByIdAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> UnsubscribeFromInterestsByIdAwaitCommandResult(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> UpdateInterestsByIdAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> UpdateInterestsByIdAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }
    }
}

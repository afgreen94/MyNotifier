using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface ICommandIssue
    {
        Task<ICallResult> IssueCommandAsync(ICommand command);
        Task<ICallResult<ICommandResult<TCommand>>> IssueCommandAwaitResultAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }

    public interface ICommandApi
    {
        Task<ICallResult> ChangeApplicationConfigurationAsync(object parameters);
        Task<ICommandResult> ChangeApplicationConfigurationAwaitCommandResultAsync(object parameters);

        Task<ICallResult> RegisterAndSubscribeToNewInterestsAsync(object parameters);
        Task<ICommandResult> RegisterAndSubscribeToNewInterestsAwaitCommandResultAsync(object parameters);

        Task<ICallResult> SubscribeToInterestsByIdAsync(object parameters);
        Task<ICommandResult> SubscribeToInterestsByIdAwaitCommandResultAsync(object parameters);

        Task<ICallResult> UnsubscribeFromInterestsByIdAsync(object parameters);
        Task<ICommandResult> UnsubscribeFromInterestsByIdAwaitCommandResult(object parameters);

        Task<ICallResult> UpdateInterestsByIdAsync(object parameters);
        Task<ICommandResult> UpdateInterestsByIdAwaitCommandResultAsync(object parameters);


        //ShutdownSystemAsync()
    }

    public interface ICommandResult { } //Guid CommandId

    public interface ICommandResult<TCommand> where TCommand : ICommand { }



    //ICommandSender
    //ICommandReciever

    //IController
    //IControllable 
}

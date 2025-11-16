using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl.Commands;
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

        Task<ICallResult> RegisterAndSubscribeToNewInterestsAsync(InterestModel[] interestModels, bool saveNew = true);
        Task<ICallResult<IRegisterAndSubscribeToNewInterestsCommandResult>> RegisterAndSubscribeToNewInterestsAwaitCommandResultAsync(InterestModel[] interestModels, bool saveNew = true);

        //these will probably get wrapped into above command later
        Task<ICallResult> SubscribeToInterestsByIdAsync(object parameters);
        Task<ICommandResult> SubscribeToInterestsByIdAwaitCommandResultAsync(object parameters);

        Task<ICallResult> UnsubscribeFromInterestsByIdAsync(Guid[] interestIds);
        Task<ICallResult<IUnsubscribeFromInterestsByIdCommandResult>> UnsubscribeFromInterestsByIdAwaitCommandResult(Guid[] interestIds);

        Task<ICallResult> UpdateInterestsByIdAsync(object parameters);
        Task<ICommandResult> UpdateInterestsByIdAwaitCommandResultAsync(object parameters);


        //ShutdownSystemAsync()
    }

    public interface ICommandResult : ICallResult { } //Guid CommandId

    public interface ICommandResult<TCommand> where TCommand : ICommand { }



    //ICommandSender
    //ICommandReciever

    //IController
    //IControllable 
}

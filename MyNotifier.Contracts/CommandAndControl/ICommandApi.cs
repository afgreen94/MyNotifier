using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface ICommandApi
    {
        Task<ICallResult> IssueCommandAsync(ICommand command);
        Task<ICallResult> IssueCommandAwaitResultAsync(ICommand command);
    }

    public interface ICommandApi0
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

    public interface ICommandResult { }

    public interface ICommandResult<TCommand> where TCommand : ICommand { }



    //ICommandSender
    //ICommandReciever

    //IController
    //IControllable 
}

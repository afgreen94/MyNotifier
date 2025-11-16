using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface IUnsubscribeFromInterestsByIdDefinition : ICommandDefinition { }
    public interface IUnsubscribeFromInterestsById : ICommand { }
    public interface IUnsubscribeFromInterestsByIdCommandResult : ICommandResult { }
    public interface IUnsubscribeFromInterestsByIdCommandParameters : ICommandParameters
    {
        Guid[] InterestIds { get; }
    }

    public interface IUnsubscribeFromInterestsByIdParameterValidator : ICommandParameterValidator<IUnsubscribeFromInterestsByIdCommandParameters> { }

    public interface IUnsubscribeFromInterestsByIdWrapper : ICommandWrapper<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters> { }
    public interface IUnsubscribeFromInterestsByIdCommandBuilder : ICommandBuilder<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters> { }
    public interface IUnsubscribeFromInterestsByIdWrapperBuilder : ICommandWrapperBuilder<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters, IUnsubscribeFromInterestsByIdWrapper> { }
}

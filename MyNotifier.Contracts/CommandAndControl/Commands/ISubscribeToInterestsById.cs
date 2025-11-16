using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface ISubscribeToInterestsByIdDefinition : ICommandDefinition { }
    public interface ISubscribeToInterestsById : ICommand { }

    public interface ISubscribeToInterestsByIdCommandParameters : ICommandParameters
    {

    }

    public interface ISubscribeToInterestsByIdWrapper : ICommandWrapper<ISubscribeToInterestsById, ISubscribeToInterestsByIdCommandParameters>
    {
        public Guid[] InterestIds { get; }
    }

    public interface ISubscribeToInterestsByIdWrapperBuilder : ICommandWrapperBuilder<ISubscribeToInterestsById, ISubscribeToInterestsByIdCommandParameters, ISubscribeToInterestsByIdWrapper> { }
}

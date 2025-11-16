using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl.Commands
{
    public class SubscribeToInterestsByIdsDefinition : ISubscribeToInterestsByIdDefinition
    {
        public Guid Id => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();
        public Type ServiceType => throw new NotImplementedException();
        public Type FactoryType => throw new NotImplementedException();
        public Type CommandType => throw new NotImplementedException();
    }
    public class SubscribeToInterestsById : ISubscribeToInterestsById
    {
        private readonly ISubscribeToInterestsByIdDefinition definition = new SubscribeToInterestsByIdsDefinition();
        public ICommandDefinition Definition => this.definition;

        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public class SubscribeToInterestsByIdWrapper :  CommandWrapper<ISubscribeToInterestsById>, ISubscribeToInterestsByIdWrapper
    {
        private readonly Guid[] interestIds;
        public Guid[] InterestIds => this.interestIds;

        public SubscribeToInterestsByIdWrapper(ICommand innerCommand, Guid[] interestIds) : base((ISubscribeToInterestsById)innerCommand) { this.interestIds = interestIds; }

    }

    public class SubscribeToInterestsByIdsWrapperBuilder : CommandWrapperBuilder<ISubscribeToInterestsById, ISubscribeToInterestsByIdWrapper>, ISubscribeToInterestsByIdWrapperBuilder
    {
        public override ICallResult<ISubscribeToInterestsByIdWrapper> BuildFrom(ICommand command, bool suppressValidation = false)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetFrom(ICommand command, out ISubscribeToInterestsByIdWrapper wrapper, out ICommandResult failedResult) => TryGetFrom<SubscribeToInterestsByIdsWrapperBuilder>(command, out wrapper, out failedResult);
    }
}

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
    public class UnsubscribeFromInterestsByIdDefinition : IUnsubscribeFromInterestsByIdDefinition
    {
        public Guid Id => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();
        public Type ServiceType => throw new NotImplementedException();
        public Type FactoryType => throw new NotImplementedException();
        public Type CommandType => throw new NotImplementedException();
    }
    public class UnsubscribeFromInterestsById : IUnsubscribeFromInterestsById
    {
        private readonly IUnsubscribeFromInterestsByIdDefinition definition = new UnsubscribeFromInterestsByIdDefinition();
        public ICommandDefinition Definition => this.definition;

        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public class UnsubscribeFromInterestsByIdCommandParameters : CommandParameters, IUnsubscribeFromInterestsByIdCommandParameters
    {
        private readonly Guid[] interestIds;

        public Guid[] InterestIds => this.interestIds;

        public UnsubscribeFromInterestsByIdCommandParameters(Guid[] interestIds) { this.interestIds = interestIds; }
    }

    public class UnsubscribeFromInterestsByIdParameterValidator : CommandParameterValidator<IUnsubscribeFromInterestsByIdCommandParameters>, IUnsubscribeFromInterestsByIdParameterValidator
    {
        public override ICallResult Validate(IUnsubscribeFromInterestsByIdCommandParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    public class UnsubscribeFromInterestsByIdWrapper : CommandWrapper<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters>, IUnsubscribeFromInterestsByIdWrapper
    {
        public UnsubscribeFromInterestsByIdWrapper(IUnsubscribeFromInterestsById command, IUnsubscribeFromInterestsByIdCommandParameters parameters) : base(command, parameters) { }
    }

    public class UnsubscribeFromInterestsByIdCommandBuilder : CommandBuilder<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters>, IUnsubscribeFromInterestsByIdCommandBuilder
    {
        public override ICallResult<IUnsubscribeFromInterestsById> BuildFrom(IUnsubscribeFromInterestsByIdCommandParameters parameters, bool suppressValidation = false)
        {
            throw new NotImplementedException();
        }
        public static bool TryGetFrom(IUnsubscribeFromInterestsByIdCommandParameters parameters, out IUnsubscribeFromInterestsById command, out ICommandResult failedResult) => TryGetFrom<UnsubscribeFromInterestsByIdCommandBuilder>(parameters, out command, out failedResult);
    }

    public class UnsubscribeFromInterestsByIdWrapperBuilder : CommandWrapperBuilder<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters, IUnsubscribeFromInterestsByIdWrapper>, IUnsubscribeFromInterestsByIdWrapperBuilder
    {
        public override ICallResult<IUnsubscribeFromInterestsByIdWrapper> BuildFrom(ICommand command, bool suppressValidation = false)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetFrom(ICommand command, out IUnsubscribeFromInterestsByIdWrapper wrapper, out ICommandResult failedResult) => TryGetFrom<UnsubscribeFromInterestsByIdWrapperBuilder>(command, out wrapper, out failedResult);   
    }
}

using MyNotifier.Contracts;
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
}

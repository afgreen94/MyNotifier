using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.CommandAndControl.Commands
{

    public class RSNI  //register and subscribe to new interests 
    {
        public class Definition { }

        public class Command { }

        public class Parameters { }

        public class ParameterValidator { }

        public class Wrapper { }

        public class Builder { }
       
        public class WrapperBuilder { }
    }

    public class RegisterAndSubscribeToNewInterestsDefinition : IRegisterAndSubscribeToNewInterestsDefinition
    {
        public static readonly Guid CommandDefinitionId = new("");
        public Guid Id => throw new NotImplementedException();
        public string Name => throw new NotImplementedException();
        public string Description => throw new NotImplementedException();
        public Type ServiceType => throw new NotImplementedException();
        public Type FactoryType => throw new NotImplementedException();
        public Type CommandType => throw new NotImplementedException();
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();
    }

    public class RegisterAndSubscribeToNewInterests : IRegisterAndSubscribeToNewInterests
    {
        private readonly IRegisterAndSubscribeToNewInterestsDefinition definition = new RegisterAndSubscribeToNewInterestsDefinition();
        public ICommandDefinition Definition => this.definition;

        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public class RegisterAndSubscribeToNewInterestsCommandParameters : CommandParameters, IRegisterAndSubscribeToNewInterestsCommandParameters
    {
        private readonly InterestModel[] interestModels;
        private readonly bool saveNew;

        public InterestModel[] InterestModels => this.interestModels;
        public bool SaveNew => this.saveNew;

        public RegisterAndSubscribeToNewInterestsCommandParameters(InterestModel[] interestModels, bool saveNew = true) : base()
        {
            this.interestModels = interestModels;
            this.saveNew = saveNew;
        }
    }

    public class RegisterAndSubscribeToNewInterestsParameterValidator : CommandParameterValidator<IRegisterAndSubscribeToNewInterestsCommandParameters>, IRegisterAndSubscribeToNewInterestsParameterValidator
    {
        public override ICallResult Validate(IRegisterAndSubscribeToNewInterestsCommandParameters parameters)
        {
            throw new NotImplementedException();
        }
    }

    public class RegisterAndSubscribeToNewInterestsWrapper : CommandWrapper<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters>, IRegisterAndSubscribeToNewInterestsWrapper
    {
        public RegisterAndSubscribeToNewInterestsWrapper(IRegisterAndSubscribeToNewInterests command, IRegisterAndSubscribeToNewInterestsCommandParameters parameters) : base(command, parameters) { }
    }

    public class RegisterAndSubscribeToNewInterestsCommandBuilder : CommandBuilder<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters>, IRegisterAndSubscribeToNewInterestsCommandBuilder
    {
        public override ICallResult<IRegisterAndSubscribeToNewInterests> BuildFrom(IRegisterAndSubscribeToNewInterestsCommandParameters parameters, bool suppressValidation = false)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetFrom(IRegisterAndSubscribeToNewInterestsCommandParameters parameters, out IRegisterAndSubscribeToNewInterests command, out ICommandResult failedResult) => TryGetFrom<RegisterAndSubscribeToNewInterestsCommandBuilder>(parameters, out command, out failedResult);
    }

    public class RegisterAndSubscribeToNewInterestsWrapperBuilder : CommandWrapperBuilder<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters, IRegisterAndSubscribeToNewInterestsWrapper>, IRegisterAndSubscribeToNewInterestsWrapperBuilder
    {
        public override ICallResult<IRegisterAndSubscribeToNewInterestsWrapper> BuildFrom(ICommand command, bool suppressValidation = false)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetFrom(ICommand command, out IRegisterAndSubscribeToNewInterestsWrapper wrapper, out ICommandResult failedResult) => TryGetFrom<RegisterAndSubscribeToNewInterestsWrapperBuilder>(command, out wrapper, out failedResult);
    }
}

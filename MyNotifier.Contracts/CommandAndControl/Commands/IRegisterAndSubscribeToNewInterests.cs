using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{

    public interface IRSNI
    {
        public interface IDefinition : ICommandDefinition { }
        public interface ICommand : CommandAndControl.ICommand { }
        public interface ICommandResult : ICommandResult<ICommand> { }
        public interface IParameters : ICommandParameters { }
        public interface IParameterValidator { }
        public interface IWrapper : ICommandWrapper<ICommand, IParameters> { }
        public interface IBuilder : ICommandBuilder<ICommand, IParameters> { }
        public interface IWrapperBuilder : ICommandWrapperBuilder<ICommand, IParameters, IWrapper> { }
    }

    public interface IRegisterAndSubscribeToNewInterestsDefinition : ICommandDefinition { }
    public interface IRegisterAndSubscribeToNewInterests : ICommand { }
    public interface IRegisterAndSubscribeToNewInterestsCommandResult : ICommandResult<IRegisterAndSubscribeToNewInterests> { }
    public interface IRegisterAndSubscribeToNewInterestsCommandParameters : ICommandParameters
    {
        InterestModel[] InterestModels { get; }
        bool SaveNew { get; }
    }
    public interface IRegisterAndSubscribeToNewInterestsParameterValidator : ICommandParameterValidator<IRegisterAndSubscribeToNewInterestsCommandParameters> { }
    public interface IRegisterAndSubscribeToNewInterestsWrapper : ICommandWrapper<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters> { }
    public interface IRegisterAndSubscribeToNewInterestsCommandBuilder : ICommandBuilder<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters> { }
    public interface IRegisterAndSubscribeToNewInterestsWrapperBuilder : ICommandWrapperBuilder<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters, IRegisterAndSubscribeToNewInterestsWrapper> { }
}

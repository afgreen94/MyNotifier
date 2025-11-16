using MyNotifier.Base;
using MyNotifier.CommandAndControl;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    public abstract class Command : ICommand
    {
        public ICommandDefinition Definition => throw new NotImplementedException();
        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public class CommandParameters : ICommandParameters
    {
    }

    public abstract class CommandParameterValidator<TCommandParameters> : ICommandParameterValidator<TCommandParameters> 
        where TCommandParameters : ICommandParameters
    {
        public abstract ICallResult Validate(TCommandParameters parameters);
    }

    //public abstract class InterestModelsCommand : Command, IInterestModelCommand { public virtual InterestModel[] InterestModels { get; set; } }
    ////public abstract class InterestDefinitionIdsCommand : Command, IInterestDefinitionIdsCommand { public virtual Guid[] InterestDefinitionIds { get; set; } }
    //public abstract class ApplicationConfigurationCommand : Command, IApplicationConfigurationCommand { public virtual IApplicationConfiguration ApplicationConfiguration { get; set; } }




    //update interest with event module(s)
    //add event module(s) to interest 
    //remove event module(s) from interest


    public abstract class CommandWrapper<TCommand, TCommandParameters>
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters
    {
        protected readonly TCommand innerCommand;
        protected readonly TCommandParameters parameters;

        public virtual TCommand InnerCommand => this.innerCommand;
        public virtual TCommandParameters Parameters => this.parameters;

        protected CommandWrapper(TCommand command, TCommandParameters parameters) { this.innerCommand = command; this.parameters = parameters; }
    }


    public abstract class CommandBuilder
    {
        public TCommand BuildFrom<TCommand, TCommandParameters>(TCommandParameters parameters)
            where TCommand : ICommand
            where TCommandParameters : ICommandParameters 
        {

            throw new NotImplementedException();
        }

        public static bool TryGetFrom<TCommand, TCommandParameters, TCommandBuilder>(TCommandParameters parameters, out TCommand command, out ICommandResult failedResult)
            where TCommand : ICommand
            where TCommandParameters : ICommandParameters
            where TCommandBuilder : class, ICommandBuilder<TCommand, TCommandParameters>, new()
        {
            command = default;
            failedResult = default;

            var builder = new TCommandBuilder();

            var buildResult = builder.BuildFrom(parameters);
            if (!buildResult.Success) { failedResult = BuildFailedWrapperBuildResult(buildResult); return false; }

            command = buildResult.Result;
            return true;
        }

        public static ICommandResult BuildFailedWrapperBuildResult(ICallResult failedWrapperBuildResult) => CallResult.BuildFailedCallResult(failedWrapperBuildResult, "Failed to build command wrapper [COMMAND DETAIL]") as CommandResult; //dont want to do it this way but...
    }

    public abstract class CommandBuilder<TCommand, TCommandParameters> : CommandBuilder, ICommandBuilder<TCommand, TCommandParameters>
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters 
        //where TWrapper : ICommandWrapper<TCommand>
    {
        //TryBuildFrom ?
        public abstract ICallResult<TCommand> BuildFrom(TCommandParameters parameters, bool suppressValidation = false);
        public static bool TryGetFrom<TCommandBuilder>(TCommandParameters parameters, out TCommand command, out ICommandResult failedResult) 
            where TCommandBuilder : class, ICommandBuilder<TCommand, TCommandParameters>, new() 
            => TryGetFrom<TCommand, TCommandParameters, TCommandBuilder>(parameters, out command, out failedResult);
    }

    public abstract class CommandWrapperBuilder
    {
        public static bool TryGetFrom<TCommand, TCommandParameters, TWrapper, TWrapperBuilder>(ICommand command, out TWrapper wrapper, out ICommandResult failedResult)
            where TCommand : ICommand
            where TCommandParameters : ICommandParameters
            where TWrapper : ICommandWrapper<TCommand, TCommandParameters>
            where TWrapperBuilder : class, ICommandWrapperBuilder<TCommand, TCommandParameters, TWrapper>, new()
        {
            wrapper = default;
            failedResult = default;

            var builder = new TWrapperBuilder();

            var buildResult = builder.BuildFrom(command);
            if (!buildResult.Success) { failedResult = BuildFailedWrapperBuildResult(buildResult); return false; }

            wrapper = buildResult.Result;
            return true;
        }

        public static ICommandResult BuildFailedWrapperBuildResult(ICallResult failedWrapperBuildResult) => CallResult.BuildFailedCallResult(failedWrapperBuildResult, "Failed to build command wrapper [COMMAND DETAIL]") as CommandResult; //dont want to do it this way but...
    }

    public abstract class CommandWrapperBuilder<TCommand, TCommandParameters, TWrapper> : CommandWrapperBuilder, ICommandWrapperBuilder<TCommand, TCommandParameters, TWrapper>
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters
        where TWrapper : ICommandWrapper<TCommand, TCommandParameters>
    {
        public abstract ICallResult<TWrapper> BuildFrom(ICommand command, bool suppressValidation = false);
        public static bool TryGetFrom<TWrapperBuilder>(ICommand command, out TWrapper wrapper, out ICommandResult failedResult) 
            where TWrapperBuilder : class, ICommandWrapperBuilder<TCommand, TCommandParameters, TWrapper>, new() 
            => TryGetFrom<TCommand, TCommandParameters, TWrapper, TWrapperBuilder>(command, out wrapper, out failedResult);
    }
}

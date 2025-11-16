using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;

namespace MyNotifier.CommandAndControl
{
    public class CommandApi : ICommandApi  
    {

        //private const string FailedToIssueInnerMessage = "Failed to issue {0} command: ";

        private readonly ICommandIssue commandIssue;
        private readonly ICallContext<CommandApi> callContext;

        public CommandApi(ICommandIssue commandIssue, ICallContext<CommandApi> callContext) { this.commandIssue = commandIssue; this.callContext = callContext; }

        public async Task<ICallResult> ChangeApplicationConfigurationAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> ChangeApplicationConfigurationAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<ICallResult> RegisterAndSubscribeToNewInterestsAsync(InterestModel[] interestModels, bool saveNew = true)
        {
            try
            {
                if (!this.TryBuildRegisterAndSubscribeToNewInterestsCommand(interestModels, saveNew, false, out var command, out var failedCallResult)) return CallResult.BuildFailedCallResult(failedCallResult, BuildFailedToIssueMessageTemplate(nameof(RegisterAndSubscribeToNewInterests)));

                return await this.commandIssue.IssueCommandAsync(command).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex, BuildFailedToIssueMessageTemplate(nameof(RegisterAndSubscribeToNewInterests))); }
        }

        public async Task<ICallResult<IRegisterAndSubscribeToNewInterestsCommandResult>> RegisterAndSubscribeToNewInterestsAwaitCommandResultAsync(InterestModel[] interestModels, bool saveNew = true) //save new by individual interest model ?
        {
            try
            {
                if (!this.TryBuildRegisterAndSubscribeToNewInterestsCommand(interestModels, saveNew, false, out var command, out var failedCallResult)) return CallResult<IRegisterAndSubscribeToNewInterestsCommandResult>.BuildFailedCallResult(failedCallResult, BuildFailedToIssueMessageTemplate(nameof(RegisterAndSubscribeToNewInterests)));

                return (ICallResult<IRegisterAndSubscribeToNewInterestsCommandResult>)await this.commandIssue.IssueCommandAwaitResultAsync(command).ConfigureAwait(false); //narrowing cast, may fail !!! 
            }
            catch (Exception ex) { return CallResult<IRegisterAndSubscribeToNewInterestsCommandResult>.FromException(ex, BuildFailedToIssueMessageTemplate(nameof(RegisterAndSubscribeToNewInterests))); }
        }

        public Task<ICallResult> SubscribeToInterestsByIdAsync(Guid[] interestIds)
        {
            throw new NotImplementedException();
        }
        public Task<ICommandResult> SubscribeToInterestsByIdAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICallResult> SubscribeToInterestsByIdAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public async Task<ICallResult> UnsubscribeFromInterestsByIdAsync(Guid[] interestIds)
        {
            try
            {
                if (!this.TryBuildUnsubscribeFromInterestsByIdCommand(interestIds, false, out var command, out var failedCallResult)) return CallResult.BuildFailedCallResult(failedCallResult, BuildFailedToIssueMessageTemplate(nameof(UnsubscribeFromInterestsById)));

                return await this.commandIssue.IssueCommandAsync(command).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex, BuildFailedToIssueMessageTemplate(nameof(UnsubscribeFromInterestsById))); }
        }

        public async Task<ICallResult<IUnsubscribeFromInterestsByIdCommandResult>> UnsubscribeFromInterestsByIdAwaitCommandResult(Guid[] interestIds)
        {
            try
            {
                if (!this.TryBuildUnsubscribeFromInterestsByIdCommand(interestIds, false, out var command, out var failedCallResult)) return CallResult<IUnsubscribeFromInterestsByIdCommandResult>.BuildFailedCallResult(failedCallResult, BuildFailedToIssueMessageTemplate(nameof(UnsubscribeFromInterestsById)));

                return (ICallResult<IUnsubscribeFromInterestsByIdCommandResult>)await this.commandIssue.IssueCommandAwaitResultAsync(command).ConfigureAwait(false);
            }
            catch(Exception ex) { return CallResult<IUnsubscribeFromInterestsByIdCommandResult>.FromException(ex, BuildFailedToIssueMessageTemplate(nameof(UnsubscribeFromInterestsById))); }
        }

        public Task<ICallResult> UpdateInterestsByIdAsync(object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> UpdateInterestsByIdAwaitCommandResultAsync(object parameters)
        {
            throw new NotImplementedException();
        }


        private bool TryBuildRegisterAndSubscribeToNewInterestsCommand(InterestModel[] interestModels, bool saveNew, bool suppressValidation, out IRegisterAndSubscribeToNewInterests command, out ICallResult failedCallResult)
        {
            var buildResult = this.BuildCommand<IRegisterAndSubscribeToNewInterests, IRegisterAndSubscribeToNewInterestsCommandParameters, RegisterAndSubscribeToNewInterestsParameterValidator, RegisterAndSubscribeToNewInterestsCommandBuilder>(new RegisterAndSubscribeToNewInterestsCommandParameters(interestModels, saveNew));

            return this.TryBuildCore(buildResult, out command, out failedCallResult);
        }

        private bool TryBuildUnsubscribeFromInterestsByIdCommand(Guid[] interestIds, bool suppressValidation, out IUnsubscribeFromInterestsById command, out ICallResult failedCallResult)
        {
            var buildResult = this.BuildCommand<IUnsubscribeFromInterestsById, IUnsubscribeFromInterestsByIdCommandParameters, UnsubscribeFromInterestsByIdParameterValidator, UnsubscribeFromInterestsByIdCommandBuilder>(new UnsubscribeFromInterestsByIdCommandParameters(interestIds), suppressValidation);

            return this.TryBuildCore(buildResult, out command, out failedCallResult);
        }

        private ICallResult<TCommand> BuildCommand<TCommand, TCommandParameters, TCommandParameterValidator, TCommandBuilder>(TCommandParameters parameters, bool suppressValidation = false)
            where TCommand : ICommand
            where TCommandParameters : ICommandParameters
            where TCommandParameterValidator : class, ICommandParameterValidator<TCommandParameters>, new()
            where TCommandBuilder : class, ICommandBuilder<TCommand, TCommandParameters>, new() => new TCommandBuilder().BuildFrom(parameters, suppressValidation);

        private bool TryBuildCore<TCommand>(ICallResult<TCommand> buildResult, out TCommand command, out ICallResult failedResult)
            where TCommand : ICommand
        {
            failedResult = buildResult;
            command = buildResult.Result;  //could break (null pointer) if failed ? should just end up null

            return buildResult.Success;
        }

        private static string BuildFailedToIssueMessageTemplate(string commandName) => $"Failed to issue command {commandName}";

        //private async Task<ICallResult> IssueCommandCoreAsync<TCommand, TCommandParameters, TCommandParametersValidator, TCommandBuilder>(TCommandParameters parameters, TCommandParametersValidator parametersValidator, TCommandBuilder builder, bool suppressValidation = false)
        //    where TCommand : ICommand
        //    where TCommandParameters : class
        //    where TCommandBuilder : ICommandBuilder<TCommand, TCommandParameters>
        //{

        //}

        //private async Task<ICallResult> IssueCommandAwaitResultCoreAsync<TCommand, TCommandParameters, TCommandParametersValidator, TCommandBuilder>(TCommandParameters parameters, TCommandParametersValidator parametersValidator, TCommandBuilder builder, bool suppressValidation = false)
        //    where TCommand : ICommand
        //    where TCommandParameters : class
        //    where TCommandBuilder : ICommandBuilder<TCommand, TCommandParameters>
        //{

        //}
    }
}

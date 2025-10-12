using MyNotifier.CommandAndControl;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Notifiers;
using static MyNotifier.Driver;
using MyNotifier.Contracts.Interests;
using MyNotifier.Contracts.Base;
using MyNotifier.Base;

namespace MyNotifier
{
    public partial class InterestPool
    {

        public class Controller : IControllable<RegisterAndSubscribeToNewInterests>,
                                  IControllable<SubscribeToInterestsById>,
                                  IControllable<UnsubscribeFromInterestsById>,
                                  IControllable<UpdateInterestsById>
        {

            private readonly InterestPool interestPool;
            private readonly Contracts.Interests.IFactory interestFactory;

            private readonly IDictionary<Guid, ICommandWrapperBuilder> commandWrapperBuilders = new Dictionary<Guid, ICommandWrapperBuilder>();

            public Contracts.Base.IDefinition Definition => throw new NotImplementedException();

            public Controller(InterestPool interestPool, Contracts.Interests.IFactory interestFactory) { this.interestPool = interestPool; this.interestFactory = interestFactory; }

            public async ValueTask<ICommandResult<RegisterAndSubscribeToNewInterests>> OnCommandAsync(RegisterAndSubscribeToNewInterests command)  //register and subscribe all given interests possible. report back errors for failed subscriptions. do not crash whole subscription process 
            {
                try
                {
                    var errorMap = new Dictionary<string, string>();

                    //wrapper build should validate commands 
                    if (!this.TryGetCommandWrapper(command, out IRegisterAndSubscribeToNewInterestsWrapper commandWrapper, out ICommandResult<ICommand> failedCommandResult)) { return (ICommandResult<RegisterAndSubscribeToNewInterests>)failedCommandResult; }

                    //put added/started up here in case of exception, interest started/added must be removed 

                    foreach (var model in commandWrapper.NewInterestModels)
                    {
                        var interestTag = $"{model.Definition.Id}_{model.Definition.Name}";

                        try
                        {
                            var getInterestResult = await this.interestFactory.GetAsync(model.EventModuleDefinitionIds, model.ParameterValues).ConfigureAwait(false);
                            if (!getInterestResult.Success) { errorMap.Add(interestTag, $"Failed to produce interest {interestTag}");  continue; }

                            var added = await this.interestPool.TryAddAsync(getInterestResult.Result).ConfigureAwait(false);
                            if (!added) { errorMap.Add(interestTag, $"Failed to add interest {interestTag} to pool."); continue; }

                            //Start Interest !!! 
                            ICallResult startResult = new CallResult();
                            if (!startResult.Success) { errorMap.Add(interestTag, $"Failed to start interest {interestTag}: {startResult.ErrorText}"); continue; } //if start fails, should remove from interest pool !!! 
                        } 
                        catch(Exception ex) { errorMap.Add(interestTag, $"Exception: {ex.Message}"); continue; } //assumes exception is non-fatal !!! 
                    }

                    return this.BuildCommandResult<RegisterAndSubscribeToNewInterests>(errorMap, commandWrapper.NewInterestModels.Length);
                }
                catch (Exception ex) { return new CommandResult<RegisterAndSubscribeToNewInterests>(false, $"Exception: {ex.Message}"); }
            }

            public async ValueTask<ICommandResult<SubscribeToInterestsById>> OnCommandAsync(SubscribeToInterestsById command)
            {
                try
                {
                    var errorMap = new Dictionary<string, string>();

                    //wrapper build should validate commands 
                    if (!this.TryGetCommandWrapper(command, out ISubscribeToInterestsByIdWrapper commandWrapper, out ICommandResult<ICommand> failedCommandResult)) { return (ICommandResult<SubscribeToInterestsById>)failedCommandResult; }

                    //put added/started up here in case of exception, interest started/added must be removed 

                    foreach(var id in commandWrapper.InterestIds)
                    {
                        try
                        {
                            var getInterestResult = await this.interestFactory.GetAsync(id).ConfigureAwait(false);
                            if(!getInterestResult.Success) { errorMap.Add(id.ToString(), $"Failed to produce interest with Id: {id}: {getInterestResult.ErrorText}"); }

                            var added = await this.interestPool.TryAddAsync(getInterestResult.Result).ConfigureAwait(false);
                            if (!added) { errorMap.Add(id.ToString(), $"Failed to add interest with Id: {id} to pool."); continue; }

                            ICallResult startResult = new CallResult();
                            if (!startResult.Success) { errorMap.Add(id.ToString(), $"Failed to start interest with Id: {id}: {startResult.ErrorText}"); }

                        }
                        catch(Exception ex) { errorMap.Add(id.ToString(), $"Exception: {ex.Message}"); continue; }
                    }

                    return this.BuildCommandResult<SubscribeToInterestsById>(errorMap, commandWrapper.InterestIds.Length);
                }
                catch (Exception ex) { return new CommandResult<SubscribeToInterestsById>(false, $"Exception: {ex.Message}"); }
            }

            public async ValueTask<ICommandResult<UnsubscribeFromInterestsById>> OnCommandAsync(UnsubscribeFromInterestsById command)
            {
                try
                {
                    var errorMap = new Dictionary<string, string>();

                    if(!this.TryGetCommandWrapper(command, out IUnsubscribeFromInterestsByIdWrapper commandWrapper, out ICommandResult<ICommand> failedCommandResult)) { return (ICommandResult<UnsubscribeFromInterestsById>)failedCommandResult; }

                    //put added/started up here in case of exception, interest started/added must be removed 


                    foreach (var id in commandWrapper.InterestIds)
                    {
                        try
                        {
                            //need to shutdown interest !!! 
                            ICallResult shutdownInterestResult = new CallResult();
                            if (!shutdownInterestResult.Success) { errorMap.Add(id.ToString(), $"Failed to shutdown interest with Id: {id}"); continue; } //critical !!! 

                            var remove = await this.interestPool.TryRemoveAsync(id).ConfigureAwait(false);
                            if (!remove) { errorMap.Add(id.ToString(), $"Failed to remove interest with Id: {id} from pool"); continue; }
                        }
                        catch(Exception ex) { errorMap.Add(id.ToString(), $"Exception: {ex.Message}"); continue; }
                    }

                    return this.BuildCommandResult<UnsubscribeFromInterestsById>(errorMap, commandWrapper.InterestIds.Length);
                }
                catch(Exception ex) { return new CommandResult<UnsubscribeFromInterestsById>(false, $"Exception: {ex.Message}"); }
            }

            public ValueTask<ICommandResult<UpdateInterestsById>> OnCommandAsync(UpdateInterestsById command)
            {
                throw new NotImplementedException();
            }

            //private async Task<ICommandResult<TCommand>> OnCommandCoreAsync<TCommand>(ICommand command, Func<ValueTask<IDictionary<string, string>>> coreLogic) where TCommand : ICommand
            //{
            //    try
            //    {
            //        //wrapper build should validate commands 
            //        if (!this.TryGetCommandWrapper(command, out IRegisterAndSubscribeToNewInterestsWrapper commandWrapper, out ICommandResult<ICommand> failedCommandResult)) { return (ICommandResult<TCommand>)failedCommandResult; }

            //        //INNER LOGIC
            //        var errorMap = await coreLogic().ConfigureAwait(false);
            //        //END INNER LOGIC 

            //        if (errorMap.Count == 0) return new CommandResult<TCommand>();
            //        else return this.BuildFailedCommandResult<TCommand>(errorMap, errorMap.Count == commandWrapper.NewInterestModels.Length);
            //    }
            //    catch (Exception ex) { return new CommandResult<TCommand>(false, $"Exception: {ex.Message}"); }
            //}

            private bool TryGetCommandWrapper<TCommandWrapper>(ICommand command, out TCommandWrapper commandWrapper, out ICommandResult<ICommand> failedCommandResult)
            {
                commandWrapper = default;
                failedCommandResult = default;

                if (this.commandWrapperBuilders[command.Definition.Id].BuildFrom(command) is not TCommandWrapper wrapper)
                {
                    failedCommandResult = (ICommandResult<ICommand>)new CommandResult<RegisterAndSubscribeToNewInterests>(false, "Failed to build command wrapper.");
                    return false;
                }

                commandWrapper = wrapper;
                return true;
            }

            private ICommandResult<TCommand> BuildCommandResult<TCommand>(IDictionary<string, string> errorMap, int allInterestsCount) where TCommand : ICommand
            {
                if (errorMap.Count == 0) return new CommandResult<TCommand>();
                else return this.BuildFailedCommandResult<TCommand>(errorMap, errorMap.Count == allInterestsCount);
            }

            private ICommandResult<TCommand> BuildFailedCommandResult<TCommand>(IDictionary<string, string> interestToErrorMap, bool failedForAllInterests) where TCommand : ICommand
            {
                throw new NotImplementedException();
            }
        }

    }
}

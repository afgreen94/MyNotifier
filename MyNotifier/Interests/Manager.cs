using MyNotifier.Base;
using MyNotifier.CommandAndControl;
using MyNotifier.CommandAndControl.Commands;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.CommandAndControl.Commands;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Interests;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MyNotifier.ApplicationForeground;
using IFactory = MyNotifier.Contracts.Interests.IFactory;
using IUpdateSubscriber = MyNotifier.Contracts.Updaters.ISubscriber;

namespace MyNotifier.Interests
{
    public class Manager : Contracts.Interests.IManager
    {
        private readonly IFactory factory;
        private readonly ICallContext<Manager> callContext;

        private readonly IControllable controllable;

        private readonly Contracts.Interests.ICache _cache;
        private readonly Cache cache = new();

        protected delegate void OnUpdateEventHandler(UpdateAvailableArgs args);
        protected event OnUpdateEventHandler onUpdateHandler;

        public IControllable Controllable => this.controllable;

        public Manager(IFactory factory, Contracts.Interests.ICache cache, ICallContext<Manager> callContext) { this.factory = factory; this._cache = cache; this.callContext = callContext; this.controllable = new Controller(this); }

        public ICallResult AddStartInterest(IInterest interest) => this.AddStartInterestCore(interest);

        public async Task<ICallResult> GetAddStartInterestAsync(Guid interestId)
        {
            try
            {
                var createInterestResult = await this.factory.GetAsync(interestId).ConfigureAwait(false);
                if (!AssertCreateResultSuccess(createInterestResult, out var failedResult)) return failedResult;

                return this.AddStartInterestCore(createInterestResult.Result);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public async Task<ICallResult> GetAddStartInterestAsync(InterestModel interest)
        {
            try
            {
                var createInterestResult = await this.factory.GetAsync(interest).ConfigureAwait(false);
                if (!AssertCreateResultSuccess(createInterestResult, out var failedResult)) return failedResult;

                return this.AddStartInterestCore(createInterestResult.Result);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public ICallResult StopRemoveInterest(Guid interestId, TimeSpan taskKillWaitTimeoutOverride = default) => this.StopRemoveInterestCore(interestId, taskKillWaitTimeoutOverride);

        public void RegisterUpdateSubscriber(IUpdateSubscriber subscriber) => this.onUpdateHandler += subscriber.OnUpdateAvailable;


        private ICallResult AddStartInterestCore(IInterest interest)
        {
            try
            {
                if (this.cache.Interests.ContainsKey(interest.Definition.Id)) return new CallResult<InterestDescription>(false, "Interest already registered.");

                var interestDescription = new InterestDescription()
                {
                    Id = interest.Definition.Id,
                    Name = interest.Definition.Name,
                    Description = interest.Definition.Description,
                    EventModuleDescriptions = new EventModuleDescription[interest.EventModules.Length]
                };

                lock (this.cache.Interests)
                {
                    this.cache.Interests.Add(interest.Definition.Id, interest);
                }

                for (int eventModuleIdx = 0; eventModuleIdx < interest.EventModules.Length; eventModuleIdx++)
                {
                    var eventModule = interest.EventModules[eventModuleIdx];

                    interestDescription.EventModuleDescriptions[eventModuleIdx] = new EventModuleDescription()
                    {
                        UpdaterDescriptions = new UpdaterDescription[eventModule.UpdaterParameterWrappers.Count]
                    };

                    for (int updaterIdx = 0; updaterIdx < eventModule.UpdaterParameterWrappers.Count; updaterIdx++)
                    {
                        var wrapper = eventModule.UpdaterParameterWrappers.ElementAt(updaterIdx).Value;

                        var backgroundTask = new UpdaterBackgroundTaskWrapper(interest, eventModule, null, null, this.OnUpdateAvailable);

                        lock (this.cache.UpdaterTasks)
                            if (!this.cache.UpdaterTasks.TryGetValue(eventModule.Id, out var updaterTasks)) this.cache.UpdaterTasks[eventModule.Id] = [backgroundTask];
                            else updaterTasks.Add(backgroundTask);

                        //register with background task manager

                        backgroundTask.Start();
                    }
                }

                return new CallResult<InterestDescription>(interestDescription);
            }
            catch (Exception ex) { return CallResult<InterestDescription>.FromException(ex); }
        }

        private ICallResult StopRemoveInterestCore(Guid interestId, TimeSpan taskKillWaitTimeoutOverride = default)
        {
            try
            {
                if (!this.cache.Interests.ContainsKey(interestId)) { return new CallResult(false, "Interest not found in cache."); }

                lock (this.cache)
                {
                    foreach (var eventModule in this.cache.Interests[interestId].EventModules)
                    {
                        var updaterTasks = this.cache.UpdaterTasks[eventModule.Id];

                        foreach (var task in updaterTasks)
                        {
                            //use background task manager ?
                            //make async ?
                            var killed = task.KillWait(taskKillWaitTimeoutOverride);

                            if (!killed) return new CallResult(false, $"Failed to kill updater task with id: {task.Id} in alloted timeout");

                            updaterTasks.Remove(task);
                        }
                    }

                    this.cache.Interests.Remove(interestId);
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        private static bool AssertCreateResultSuccess(ICallResult createResult, out ICallResult<InterestDescription> failedResult)
        {
            failedResult = default;
            if (!createResult.Success) { failedResult = CallResult<InterestDescription>.BuildFailedCallResult(createResult, "Failed to Add Interest"); return false; }
            return true;
        }

        private void OnUpdateAvailable(UpdaterBackgroundTaskWrapper task, IUpdaterResult result)
        {
            this.onUpdateHandler(new UpdateAvailableArgs()
            {
                Interest = task.Interest,
                EventModule = task.EventModule,
                //Updater = task.Updater,
                Result = result
            });
        }

        public class UpdaterDescription : Contracts.Base.Definition
        {
            public Contracts.Updaters.IDefinition UpdaterDefinition { get; set; }
            public Parameter[] Parameters { get; set; }
        }

        public class EventModuleDescription : Contracts.Base.Definition
        {
            public Contracts.EventModules.IDefinition EventModuleDefinition { get; set; }
            public UpdaterDescription[] UpdaterDescriptions { get; set; }
        }

        public class InterestDescription : Contracts.Base.Definition
        {
            public EventModuleDescription[] EventModuleDescriptions { get; set; }
        }

        private class Cache
        {
            public IDictionary<Guid, IInterest> Interests = new Dictionary<Guid, IInterest>();
            public IDictionary<Guid, HashSet<UpdaterBackgroundTaskWrapper>> UpdaterTasks = new Dictionary<Guid, HashSet<UpdaterBackgroundTaskWrapper>>();
        }


        #region Updater Task
        protected class UpdaterBackgroundTaskWrapper : BackgroundTaskWrapper
        {
            private readonly IInterest interest;
            private readonly IEventModule eventModule;
            private readonly StaticUpdater updater; //updater should be available thru harness
            private readonly ITaskSettings taskSettings;
            ///private readonly int delayParameter; //milliseconds

            protected event OnUpdateEventHandler OnUpdateHandler;

            public IInterest Interest => this.interest;
            public IEventModule EventModule => this.eventModule;
            public StaticUpdater Updater => this.updater;


            public delegate void OnUpdateEventHandler(UpdaterBackgroundTaskWrapper sender, IUpdaterResult updaterResult);

            public UpdaterBackgroundTaskWrapper(IInterest interest, 
                                                IEventModule eventModule, 
                                                StaticUpdater updater, 
                                                ITaskSettings taskSettings,
                                                OnUpdateEventHandler onUpdateDelegate)
            {
                this.interest = interest;
                this.eventModule = eventModule;
                this.updater = updater;
                this.taskSettings = taskSettings;
                //this.delayParameter = delayParameter; //pull delay parameter from updater parameters

                this.OnUpdateHandler += onUpdateDelegate;
            }

            protected override async ValueTask ActionAsync()
            {
                while (!this.killFlag.Value)
                {
                    try
                    {
                        var result = await this.updater.TryGetUpdateAsync().ConfigureAwait(false); //want parameterized updater, abstract away stripping parameters / loop delay parameter 
                        if (!result.Success)
                        {
                            var failedResult = CallResult.BuildFailedCallResult(result, "TryGetUpdateAsync failure");
                            var handleFailureArgs = await this.OnFailureAsync(failedResult).ConfigureAwait(false);

                            //?
                        }

                        if (result.UpdateAvailable) this.OnUpdateAvailable(result);

                        await Task.Delay(this.taskSettings.DelayMilliseconds).ConfigureAwait(false);
                    }
                    catch (Exception ex) 
                    {
                        var handleFailureArgs = await this.OnFailureAsync(CallResult.FromException(ex)).ConfigureAwait(false);
                        throw new NotImplementedException();
                    } //OnExceptionAsync() ?
                }
            }

            private void OnUpdateAvailable(IUpdaterResult result) => this.OnUpdateHandler(this, result);
            private async ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failureResult) => throw new NotImplementedException();
        }

        #endregion #Updater Task


        #region Controllable


        public class Controller : IControllable
        {
            private readonly IManager manager;

            private readonly Contracts.Base.Definition definition = new() { };
            public Contracts.Base.IDefinition Definition => this.definition;

            public Controller(IManager manager) { this.manager = manager; }

            public async ValueTask<ICommandResult> OnCommandAsync(ICommand command)
            {
                if (command.Definition.CommandType == typeof(RegisterAndSubscribeToNewInterests))
                {

                    if (!RegisterAndSubscribeToNewInterestsWrapperBuilder.TryGetFrom(command, out var wrapper, out var failedResult)) return failedResult;

                    foreach(var interest in wrapper.Parameters.InterestModels)
                    {
                        var getAddStartInterestResult = await this.manager.GetAddStartInterestAsync(interest).ConfigureAwait(false);
                        if(!getAddStartInterestResult.Success) 
                        {

                            //how to handle individual failure ?
                            //make list and build composite result?
                            //fail on any individual failure?
                            //TBD !!! 

                            return CallResult.BuildFailedCallResult(getAddStartInterestResult, "Failed to add new interest by model [DETAIL]") as CommandResult;
                        }
                    }

                    return new CommandResult();
                }
                //else if (command.Definition.CommandType == typeof(SubscribeToInterestsById))
                //{
                //    if (!SubscribeToInterestsByIdsWrapperBuilder.TryGetFrom(command, out var wrapper, out var failedResult)) return failedResult;

                //    foreach(var interestId in wrapper.InterestIds)
                //    {
                //        var getAddStartInterestResult = await this.manager.GetAddStartInterestAsync(interestId).ConfigureAwait(false);
                //        if (!getAddStartInterestResult.Success) { /* same as above !!! for now just crash all*/ return CallResult.BuildFailedCallResult(getAddStartInterestResult, "Failed to add new interest by Id [DETAIL]: {0}") as CommandResult; }

                //    }
                //}
                else if (command.Definition.CommandType == typeof(UnsubscribeFromInterestsById))
                {
                    if (!UnsubscribeFromInterestsByIdWrapperBuilder.TryGetFrom(command, out var wrapper, out var failedResult)) return failedResult;

                    foreach(var interestId in wrapper.Parameters.InterestIds)
                    {
                        var stopRemoveInterestResult = this.manager.StopRemoveInterest(interestId); //include timespan ? use default ?
                        
                        if(!stopRemoveInterestResult.Success) 
                        {
                            //same as above
                            return CallResult.BuildFailedCallResult(stopRemoveInterestResult, "Failed to remove interest by Id [DETAIL]") as CommandResult; 
                        }
                    }
                }
                else if (command.Definition.CommandType == typeof(UpdateInterestsById))
                {

                }
                else return new CommandResult(false, $"Unsupported command type: {command.Definition.CommandType}");


                return new CommandResult();
            }
        }




        #endregion Controllable
    }
}

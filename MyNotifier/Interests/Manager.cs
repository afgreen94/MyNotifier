using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
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

namespace MyNotifier.Interests
{
    public class Manager : Contracts.Interests.IManager
    {
        private readonly IFactory factory;
        private readonly ICallContext<Manager> callContext;

        private readonly Contracts.Interests.ICache _cache;
        private readonly Cache cache = new();

        //protected delegate void OnUpdateEventHandler(UpdaterHarness sender, Notification notification);
        //protected event OnUpdateEventHandler subscriptions;

        public Manager(IFactory factory, Contracts.Interests.ICache cache, ICallContext<Manager> callContext) { this.factory = factory; this._cache = cache; this.callContext = callContext; }


        public ICallResult<InterestDescription> AddStartInterest(IInterest interest) => this.AddStartInterestCore(interest);

        public async Task<ICallResult<InterestDescription>> GetAddStartInterestAsync(InterestModel interest)
        {
            try
            {
                var createInterestResult = await this.factory.GetAsync(interest).ConfigureAwait(false);
                if (!AssertCreateResultSuccess(createInterestResult, out var failedResult)) return failedResult;

                return this.AddStartInterestCore(createInterestResult.Result);
            }
            catch (Exception ex) { return CallResult<InterestDescription>.FromException(ex); }
        }

        public async Task<ICallResult<InterestDescription>> GetAddStartInterestAsync(Guid interestId)
        {
            try
            {
                var createInterestResult = await this.factory.GetAsync(interestId).ConfigureAwait(false);
                if (!AssertCreateResultSuccess(createInterestResult, out var failedResult)) return failedResult;

                return this.AddStartInterestCore(createInterestResult.Result);
            }
            catch(Exception ex) { return CallResult<InterestDescription>.FromException(ex); }
        }

        public ICallResult StopRemoveInterestAsync(IInterest interest) => this.StopRemoveInterestCore(interest.Definition.Id);
        public ICallResult StopRemoveInterestAsync(Guid interestId) =>this.StopRemoveInterestCore(interestId);
        public ICallResult StopRemoveInterestAsync(InterestDescription interest) => this.StopRemoveInterestCore(interest.Definition.Id);
        
        private ICallResult<InterestDescription> AddStartInterestCore(IInterest interest)
        {
            try
            {

                if (this.cache.Interests.ContainsKey(interest.Definition.Id)) return new CallResult<InterestDescription>(false, "Interest already registered.");

                var interestDescription = new InterestDescription()
                {
                    Definition = new Contracts.Base.Definition()
                    {
                        Id = interest.Definition.Id,
                        Name = interest.Definition.Name,
                        Description = interest.Definition.Description
                    },
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
                        {
                            if (!this.cache.UpdaterTasks.TryGetValue(eventModule.Id, out var updaterTasks)) this.cache.UpdaterTasks[eventModule.Id] = [backgroundTask];
                            updaterTasks.Add(backgroundTask);
                        }

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
                    foreach(var eventModule in this.cache.Interests[interestId].EventModules)
                    {
                        var updaterTasks = this.cache.UpdaterTasks[eventModule.Id];

                        foreach(var task in updaterTasks)
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
            if (!createResult.Success) { failedResult = CallResult<InterestDescription>.BuildFailedCallResult(createResult, "Failed to Add Interest: {0}"); return false; }
            return true;
        }


        private IUpdateSubscriber[] subscribers;
        public void RegisterSubscriber(IUpdateSubscriber subscriber)
        {

        }
        private void OnUpdateAvailable(UpdaterBackgroundTaskWrapper task, IUpdaterResult result)
        {

            foreach (var subscriber in this.subscribers) subscriber.OnUpdateAvailable(new UpdateAvailableArgs()
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

        public class InterestDescription 
        {
            public Contracts.Base.IDefinition Definition { get; set; }

            public EventModuleDescription[] EventModuleDescriptions { get; set; }
        }

        protected class Set
        {
            public IInterest Interest { get; set; }
            public IEventModule EventModule { get; set; }
            public IUpdater Updater { get; set; }

            public UpdaterBackgroundTaskWrapper Task { get; set; }

            public string Hash => $"{this.Interest.Definition.Id}_{this.EventModule.Definition.Id}_{this.Updater.Definition.Id}"; //need parameters hash //temporary, this is not an appropriate hash
        }

        private class Cache
        {
            public IDictionary<Guid, IInterest> Interests = new Dictionary<Guid, IInterest>();
            public IDictionary<Guid, HashSet<UpdaterBackgroundTaskWrapper>> UpdaterTasks = new Dictionary<Guid, HashSet<UpdaterBackgroundTaskWrapper>>();
        }

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

            public UpdaterBackgroundTaskWrapper(IInterest interest, IEventModule eventModule, StaticUpdater updater, ITaskSettings taskSettings, OnUpdateEventHandler onUpdateDelegate)
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
                            var failedResult = CallResult.BuildFailedCallResult(result, "TryGetUpdateAsync failure: {0}");
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


        //private class HarnessCancelFlagWrapper
        //{
        //    public UpdaterHarness Harness { get; set; }
        //    public BooleanFlag CancelFlag { get; set; }

        //    public void Start() => this.Harness.Start(this.CancelFlag);
        //    public void Stop() => this.CancelFlag.Value = false; //await harness task stoppage !!! //done non-thematically !!! //fix this 
        //}

        //private static string GetHash(IInterest interest, IEventModule eventModule, IUpdater updater, Parameter[] parameters) => $"{interest.Definition.Id}_{eventModule.Definition.Id}_{updater.Definition.Id}"; //need parameters hash 
    }

    //public class UpdaterHarness(IInterest interest,
    //                            IEventModule eventModule,
    //                            IUpdater updater,
    //                            Parameter[] parameters,
    //                            Handler oldhandler) //IBackgrounder 
    //{
    //    private readonly IInterest interest = interest;
    //    private readonly IEventModule eventModule = eventModule;
    //    private readonly IUpdater updater = updater;
    //    private readonly Parameter[] parameters = parameters;
    //    private readonly Handler oldhandler = oldhandler;
    //    private readonly int delayParameterMs = 0; //from updater parameters 

    //    private Task task;
    //    private BooleanFlag currentCancelFlag;
    //    private bool running = false;

    //    private IDictionary<Guid, IUpdateSubscriber> subscribers = new Dictionary<Guid, IUpdateSubscriber>();
    //    private delegate void OnUpdateAvailableHandler(UpdateAvailableArgs update);
    //    private event OnUpdateAvailableHandler onUpdateAvailableHandler;


    //    private string hash;
    //    public string Hash
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(this.hash)) { this.hash = $"{this.interest.Definition.Id}_{this.eventModule.Definition.Id}_{this.updater.Definition.Id}"; }
    //            return this.hash;
    //        }
    //    }

    //    //AddHandler()

    //    public ICallResult Start(BooleanFlag cancelFlag)
    //    {
    //        try
    //        {
    //            if (this.running) return new CallResult(false, "Already running.");

    //            this.SetDelay();

    //            this.currentCancelFlag = cancelFlag;

    //            this.task = Task.Run(async () =>
    //            {
    //                this.running = true;

    //                while (!this.currentCancelFlag.Value)
    //                {
    //                    try
    //                    {
    //                        var result = await this.updater.TryGetUpdateAsync(this.parameters).ConfigureAwait(false); //want parameterized updater, abstract away stripping parameters / loop delay parameter 

    //                        if (!result.Success)
    //                        {
    //                            var failedResult = CallResult.BuildFailedCallResult(result, "TryGetUpdateAsync failure: {0}");

    //                            var handleFailureArgs = await this.OnFailureAsync(failedResult).ConfigureAwait(false);

    //                            //?
    //                        }

    //                        if (result.UpdateAvailable) this.OnUpdateAvailable(result);

    //                        await Task.Delay(this.delayParameterMs).ConfigureAwait(false);
    //                    }
    //                    catch (Exception ex) { await this.OnFailureAsync(CallResult.FromException(ex)).ConfigureAwait(false); } //OnExceptionAsync() ?
    //                }
    //            });

    //            return new CallResult();
    //        }
    //        catch (Exception ex) { return CallResult.FromException(ex); }
    //    }

    //    private readonly TimeSpan defaultTimeout = new(0, 1, 0);
    //    public async Task<ICallResult> StopWaitForCompleteAsync(TimeSpan timeoutOverride = default)
    //    {
    //        try
    //        {
    //            if (!this.running) return new CallResult(false, "Not running.");

    //            this.currentCancelFlag.Value = false;

    //            await this.task.WaitAsync((timeoutOverride != default ? timeoutOverride : this.defaultTimeout)).ConfigureAwait(false);

    //            if (!this.task.IsCompleted) return new CallResult(false, "Failed to kill harness task.");
    //            if (!this.task.IsFaulted) return new CallResult(false, $"Harness task faulted: {((this.task.Exception != null && !string.IsNullOrEmpty(this.task.Exception.Message)) ? this.task.Exception.Message : string.Empty)}");

    //            this.Reset();

    //            return new CallResult();
    //        }
    //        catch (Exception ex) { return CallResult.FromException(ex); }
    //    }


    //    public ICallResult Subscribe(IUpdateSubscriber subscriber)
    //    {
    //        try
    //        {
    //            if (this.subscribers.ContainsKey(subscriber.Id)) return new CallResult(false, "Subsciber already subscribed.");

    //            this.onUpdateAvailableHandler += subscriber.OnUpdateAvailable;
    //            this.subscribers.Add(subscriber.Id, subscriber);

    //            return new CallResult();
    //        }
    //        catch (Exception ex) { return CallResult.FromException(ex); }
    //    }

    //    public ICallResult Unsubscribe(Guid id)
    //    {
    //        try
    //        {
    //            if (!this.subscribers.TryGetValue(id, out IUpdateSubscriber? subscriber)) return new CallResult(false, "Subscriber not subscribed.");

    //            this.onUpdateAvailableHandler -= subscriber.OnUpdateAvailable;
    //            this.subscribers.Remove(id);

    //            return new CallResult();
    //        }
    //        catch(Exception ex) { return CallResult.FromException(ex); }
    //    }


    //    private void SetDelay() { }

    //    private void Reset() //idk about this //unregister background task from IBackgrounder 
    //    {
    //        this.task.Dispose(); this.task = null; //?
    //        this.currentCancelFlag = null;

    //        this.running = false;
    //    }

    //    private void OnUpdateAvailable(IUpdaterResult result) => this.onUpdateAvailableHandler(new() //need new one every time ? //use struct ? 
    //    {
    //        Interest = this.interest,
    //        EventModule = this.eventModule,
    //        Updater = this.updater,
    //        Result = result
    //    });

    //    private async ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failureResult) => await this.oldhandler.OnFailureAsync(failureResult).ConfigureAwait(false);

    //}
}

using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Interests;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.ApplicationForeground;
using IFactory = MyNotifier.Contracts.Interests.IFactory;

namespace MyNotifier.Interests
{
    public class Manager : Contracts.Interests.IManager
    {
        private readonly IFactory factory;
        private readonly ICallContext<Manager> callContext;

        private readonly Cache cache = new();

        protected delegate void OnUpdateEventHandler(UpdaterHarness sender, Notification notification);
        protected event OnUpdateEventHandler subscriptions;

        public Manager(IFactory factory, Contracts.Interests.ICache cache, ICallContext<Manager> callContext) { this.factory = factory; this.callContext = callContext; }

        public ICallResult AddStartInterest(IInterest interest, IUpdateSubscriber[] subscribers, BooleanFlag cancelFlag)
        {
            try
            {
                foreach (var eventModule in interest.EventModules)
                    foreach (var wrapper in eventModule.UpdaterParameterWrappers.Values)
                    {
                        var harnessHash = GetHash(interest, eventModule, wrapper.Updater, wrapper.Parameters);

                        if (this.cache.HarnessHashHarnessesMap.ContainsKey(harnessHash)) return new CallResult(false, "harness already added.");

                        var harness = new UpdaterHarness(interest,
                                                         eventModule,
                                                         wrapper.Updater,
                                                         wrapper.Parameters,
                                                         null);

                        foreach (var subscriber in subscribers) harness.Subscribe(subscriber);

                        this.cache.InterestDefinitionIdToSubscribersMap.Add(interest.Definition.Id, subscribers);

                        var harnessWrapper = new HarnessCancelFlagWrapper() { Harness = harness, CancelFlag = cancelFlag };

                        harnessWrapper.Start();

                        //locking?
                        lock (this.cache)
                        {
                            this.cache.HarnessHashHarnessesMap.Add(harnessHash, harnessWrapper);

                            if (this.cache.InterestIdUpdaterHarnessSetMap.TryGetValue(interest.Definition.Id, out var set)) set.Add(harnessWrapper);
                            else this.cache.InterestIdUpdaterHarnessSetMap.Add(interest.Definition.Id, [harnessWrapper]);
                        }
                    }

                return new CallResult();

            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public ICallResult StopRemoveInterest(IInterest interest, Handler handler, BooleanFlag cancelFlag)
        {
            try
            {
                if (!this.cache.InterestIdUpdaterHarnessSetMap.ContainsKey(interest.Definition.Id)) return new CallResult(false, "Interest not found in cache."); //TryGet before lock risks race condition ?!?!?!

                lock (this.cache)
                {
                    foreach (var wrapper in this.cache.InterestIdUpdaterHarnessSetMap[interest.Definition.Id])
                    {
                        wrapper.Stop();
                        this.cache.HarnessHashHarnessesMap.Remove(wrapper.Harness.Hash);
                    }

                    this.cache.InterestIdUpdaterHarnessSetMap.Remove(interest.Definition.Id);
                }

                return new CallResult();

            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        private class Cache
        {
            public IDictionary<string, HarnessCancelFlagWrapper> HarnessHashHarnessesMap = new Dictionary<string, HarnessCancelFlagWrapper>();
            public IDictionary<Guid, HashSet<HarnessCancelFlagWrapper>> InterestIdUpdaterHarnessSetMap = new Dictionary<Guid, HashSet<HarnessCancelFlagWrapper>>();
            public IDictionary<Guid, IUpdateSubscriber[]> InterestDefinitionIdToSubscribersMap = new Dictionary<Guid, IUpdateSubscriber[]>();
        }

        private class HarnessCancelFlagWrapper
        {
            public UpdaterHarness Harness { get; set; }
            public BooleanFlag CancelFlag { get; set; }

            public void Start() => this.Harness.Start(this.CancelFlag);
            public void Stop() => this.CancelFlag.Value = false; //await harness task stoppage !!! //done non-thematically !!! //fix this 
        }

        private static string GetHash(IInterest interest, IEventModule eventModule, IUpdater updater, Parameter[] parameters) => $"{interest.Definition.Id}_{eventModule.Definition.Id}_{updater.Definition.Id}"; //need parameters hash 


    }

    public class UpdaterHarness(IInterest interest,
                                IEventModule eventModule,
                                IUpdater updater,
                                Parameter[] parameters,
                                Handler oldhandler) //IBackgrounder 
    {
        private readonly IInterest interest = interest;
        private readonly IEventModule eventModule = eventModule;
        private readonly IUpdater updater = updater;
        private readonly Parameter[] parameters = parameters;
        private readonly Handler oldhandler = oldhandler;
        private readonly int delayParameterMs = 0; //from updater parameters 

        private Task task;
        private BooleanFlag currentCancelFlag;
        private bool running = false;

        private IDictionary<Guid, IUpdateSubscriber> subscribers = new Dictionary<Guid, IUpdateSubscriber>();
        private delegate void OnUpdateAvailableHandler(UpdateAvailableArgs update);
        private event OnUpdateAvailableHandler onUpdateAvailableHandler;


        private string hash;
        public string Hash
        {
            get
            {
                if (string.IsNullOrEmpty(this.hash)) { this.hash = $"{this.interest.Definition.Id}_{this.eventModule.Definition.Id}_{this.updater.Definition.Id}"; }
                return this.hash;
            }
        }

        //AddHandler()

        public ICallResult Start(BooleanFlag cancelFlag)
        {
            try
            {
                if (this.running) return new CallResult(false, "Already running.");

                this.SetDelay();

                this.currentCancelFlag = cancelFlag;

                this.task = Task.Run(async () =>
                {
                    this.running = true;

                    while (!this.currentCancelFlag.Value)
                    {
                        try
                        {
                            var result = await this.updater.TryGetUpdateAsync(this.parameters).ConfigureAwait(false); //want parameterized updater, abstract away stripping parameters / loop delay parameter 

                            if (!result.Success)
                            {
                                var failedResult = CallResult.BuildFailedCallResult(result, "TryGetUpdateAsync failure: {0}");

                                var handleFailureArgs = await this.OnFailureAsync(failedResult).ConfigureAwait(false);

                                //?
                            }

                            if (result.UpdateAvailable) this.OnUpdateAvailable(result);

                            await Task.Delay(this.delayParameterMs).ConfigureAwait(false);
                        }
                        catch (Exception ex) { await this.OnFailureAsync(CallResult.FromException(ex)).ConfigureAwait(false); } //OnExceptionAsync() ?
                    }
                });

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        private readonly TimeSpan defaultTimeout = new(0, 1, 0);
        public async Task<ICallResult> StopWaitForCompleteAsync(TimeSpan timeoutOverride = default)
        {
            try
            {
                if (!this.running) return new CallResult(false, "Not running.");

                this.currentCancelFlag.Value = false;

                await this.task.WaitAsync((timeoutOverride != default ? timeoutOverride : this.defaultTimeout)).ConfigureAwait(false);

                if (!this.task.IsCompleted) return new CallResult(false, "Failed to kill harness task.");
                if (!this.task.IsFaulted) return new CallResult(false, $"Harness task faulted: {((this.task.Exception != null && !string.IsNullOrEmpty(this.task.Exception.Message)) ? this.task.Exception.Message : string.Empty)}");

                this.Reset();

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }


        public ICallResult Subscribe(IUpdateSubscriber subscriber)
        {
            try
            {
                if (this.subscribers.ContainsKey(subscriber.Id)) return new CallResult(false, "Subsciber already subscribed.");

                this.onUpdateAvailableHandler += subscriber.OnUpdateAvailable;
                this.subscribers.Add(subscriber.Id, subscriber);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public ICallResult Unsubscribe(Guid id)
        {
            try
            {
                if (!this.subscribers.TryGetValue(id, out IUpdateSubscriber? subscriber)) return new CallResult(false, "Subscriber not subscribed.");

                this.onUpdateAvailableHandler -= subscriber.OnUpdateAvailable;
                this.subscribers.Remove(id);

                return new CallResult();
            }
            catch(Exception ex) { return CallResult.FromException(ex); }
        }


        private void SetDelay() { }

        private void Reset() //idk about this //unregister background task from IBackgrounder 
        {
            this.task.Dispose(); this.task = null; //?
            this.currentCancelFlag = null;

            this.running = false;
        }

        private void OnUpdateAvailable(IUpdaterResult result) => this.onUpdateAvailableHandler(new() //need new one every time ? //use struct ? 
        {
            Interest = this.interest,
            EventModule = this.eventModule,
            Updater = this.updater,
            Result = result
        });

        private async ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failureResult) => await this.oldhandler.OnFailureAsync(failureResult).ConfigureAwait(false);
    }
}

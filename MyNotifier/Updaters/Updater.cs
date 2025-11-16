using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.Notifications;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Updaters
{

    //currently 
    //update logic is invoked dynamically eg updater.Update(params)
    //updater should be static eg updater.initialize(params) ... updater.update()
    //static updater can then be wrapped for dynamic use eg wrapper.update(params) => updater.init(params); updater.update()
    //default updater should be static, with dynamic updater wrapper as a convenience
    //TBD !!! 
    public abstract class Updater : IUpdater
    {

        protected readonly ICallContext<Updater> callContext;

        protected bool isInitialized = false;
        protected Parameter[] parameters; //should be dictionary? easier? 

        public abstract IUpdaterDefinition Definition { get; }

        public Updater(ICallContext<Updater> callContext) { this.callContext = callContext; }

        public virtual async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false)
        {
            if (this.isInitialized && !forceReinitialize) return new CallResult();

            try
            {
                await this.InitializeCoreAsync().ConfigureAwait(false);

                this.isInitialized = true;

                return new CallResult();
            }
            catch (Exception ex) { return new CallResult(false, ex.Message); }
        } 

        public virtual async ValueTask<IUpdaterResult> TryGetUpdateAsync(Parameter[] parameters) //maybe want to pass parameters here. init with params is fine but loading updaters with params could get redundant/confusing with different interests using same updaters and different params 
        {
            try
            {
                var updateAvailable = await this.CheckUpdateAvailableAsync(parameters).ConfigureAwait(false); //may need proxyiomanager for checking state file 
                if (!updateAvailable) return new Result() { UpdateAvailable = false };


                return await this.RetrieveUpdateAsync(parameters).ConfigureAwait(false);  //need to update state ! 
            }
            catch (Exception ex) { return new Result(false, ex.Message); }
        }

        protected abstract ValueTask InitializeCoreAsync();
        protected abstract ValueTask<bool> CheckUpdateAvailableAsync(Parameter[] parameters);
        protected abstract ValueTask<IUpdaterResult> RetrieveUpdateAsync(Parameter[] parameters);


        public class Result : CallResult, IUpdaterResult
        {

            public Result() { }
            public Result(bool updateAvailable, 
                          DateTime updatedAt, 
                          byte[] data, 
                          DataTypeArgs typeArgs)
            {
                UpdateAvailable = updateAvailable;
                UpdatedAt = updatedAt;
                Data = data;
                TypeArgs = typeArgs;
            }

            public Result(bool succeeded, string errorText) : base(succeeded, errorText) { }

            public bool UpdateAvailable { get; set; }
            public DateTime UpdatedAt { get; set; }
            public byte[] Data { get; set; }
            public DataTypeArgs TypeArgs { get; set; }
        }
    }


    public interface IUpdaterBase
    {
        IUpdaterDefinition Definition { get; }
    }

    public interface IStaticUpdater : IUpdaterBase
    {
        ValueTask<ICallResult> InitializeAsync(Parameter[] parameters, bool forceReinitialize = false);
        ValueTask<IUpdaterResult> TryGetUpdateAsync();
    }

    public interface IDynamicUpdater : IUpdaterBase
    {
        ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false);
        ValueTask<IUpdaterResult> TryGetUpdateAsync(Parameter[] parameters);
    }

    public abstract class StaticUpdater : IStaticUpdater
    {

        private readonly ICallContext<StaticUpdater> callContext;

        private bool isInitialized = false;

        protected abstract IUpdaterDefinition updaterDefinition { get; }
        public virtual IUpdaterDefinition Definition => this.updaterDefinition;

        public StaticUpdater(ICallContext<StaticUpdater> callContext) { this.callContext = callContext; }

        public async ValueTask<ICallResult> InitializeAsync(Parameter[] parameters, bool forceReinitialize = false)
        {
            try
            {
                if (this.isInitialized && !forceReinitialize) return new CallResult();

                var parameterizeResult = this.Parameterize(parameters);
                if (!parameterizeResult.Success) return CallResult.BuildFailedCallResult(parameterizeResult, "Failed to parameterize updater");

                var initializeCoreResult = await this.InitializeCoreAsync().ConfigureAwait(false);
                if (!initializeCoreResult.Success) return CallResult.BuildFailedCallResult(initializeCoreResult, "Initialize core call failed");

                this.isInitialized = true;

                return new CallResult();

            } catch(Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual async ValueTask<IUpdaterResult> TryGetUpdateAsync()
        {
            try
            {
                var updateAvailable = await this.CheckUpdateAvailableAsync().ConfigureAwait(false); //may need proxyiomanager for checking state file 
                if (!updateAvailable) return new Result() { UpdateAvailable = false };


                var retrieveUpdaterResult = await this.RetrieveUpdateAsync().ConfigureAwait(false);  //need to update state ! 
                if (!retrieveUpdaterResult.Success) return (Result)CallResult.BuildFailedCallResult(retrieveUpdaterResult, "Failed to retrieve update"); //casting will fail!

                var writeStateArg = new object();
                var writeStateResult = await this.WriteStateAsync(writeStateArg).ConfigureAwait(false);
                if (!writeStateResult.Success) return (Result)CallResult.BuildFailedCallResult(writeStateResult, "Failed to write updater state"); //casting will fail!

                return retrieveUpdaterResult;
            }
            catch(Exception ex) { return (Result)CallResult.FromException(ex); } //? //casting will fail!
        }


        protected virtual ValueTask<ICallResult> InitializeCoreAsync() => new(new CallResult());
        protected virtual ICallResult Parameterize(Parameter[] parameters) { return new CallResult(); } //validate and set updater parameters 


        protected abstract ValueTask<bool> CheckUpdateAvailableAsync();
        protected abstract ValueTask<IUpdaterResult> RetrieveUpdateAsync();


        protected virtual ValueTask<ICallResult> RetrieveStateAsync(object retrieveStateArg) => throw new NotImplementedException();
        protected virtual ValueTask<ICallResult> WriteStateAsync(object writeStateArg) => throw new NotImplementedException();


        public class Result : CallResult, IUpdaterResult
        {

            public Result() { }
            public Result(bool updateAvailable,
                          DateTime updatedAt,
                          byte[] data,
                          DataTypeArgs typeArgs)
            {
                UpdateAvailable = updateAvailable;
                UpdatedAt = updatedAt;
                Data = data;
                TypeArgs = typeArgs;
            }

            public Result(bool succeeded, string errorText) : base(succeeded, errorText) { }

            public bool UpdateAvailable { get; set; }
            public DateTime UpdatedAt { get; set; }
            public byte[] Data { get; set; }
            public DataTypeArgs TypeArgs { get; set; }
        }
    }


    public class DynamicUpdater : IDynamicUpdater
    {

        private readonly StaticUpdater updater;

        public IUpdaterDefinition Definition => this.updater.Definition;

        public ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<IUpdaterResult> TryGetUpdateAsync(Parameter[] parameters)
        {
            try
            {

                var reInitializeResult = await this.updater.InitializeAsync(parameters, true).ConfigureAwait(false);
                if (!reInitializeResult.Success) return (IUpdaterResult)CallResult.BuildFailedCallResult(reInitializeResult, "Failed to reinitialize updater with new parameters");

                return await this.updater.TryGetUpdateAsync().ConfigureAwait(false);
            }
            catch (Exception ex) { return (IUpdaterResult)CallResult.FromException(ex); } //cast will fail
        }
    }
}

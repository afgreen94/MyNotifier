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

namespace MyNotifier.Updaters
{
    public abstract class Updater : IUpdater
    {

        protected bool isInitialized = false;
        protected Parameter[] parameters; //should be dictionary? easier? 

        public abstract IUpdaterDefinition Definition { get; }

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
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Updaters
{
    public class Factory : MyNotifier.Contracts.Updaters.IFactory
    {
        private readonly IDefinitionProvider definitionProvider;
        private readonly IModuleLoader moduleLoader;
        private readonly Contracts.Updaters.ICache cache;
        private readonly IConfiguration configuration;
        private readonly ICallContext<Factory> callContext;

        public Factory(IDefinitionProvider definitionProvider,
                              IModuleLoader moduleLoader,
                              Contracts.Updaters.ICache cache,
                              IConfiguration configuration,
                              ICallContext<Factory> callContext)
        {
            this.definitionProvider = definitionProvider;
            this.moduleLoader = moduleLoader;
            this.cache = cache;
            this.configuration = configuration;
            this.callContext = callContext;
        }

        //public async ValueTask<ICallResult> InitializeAsync() => throw new NotImplementedException();

        public async ValueTask<ICallResult<IUpdater>> GetAsync(Guid updaterDefinitionId)
        {
            try
            {
                if (this.cache.TryGetValue(updaterDefinitionId, out IUpdater cachedUpdater)) return new CallResult<IUpdater>(cachedUpdater);

                var getUpdaterDefinitionResult = await this.GetDefinitionCoreAsync(updaterDefinitionId).ConfigureAwait(false);
                if (!getUpdaterDefinitionResult.Success) return CallResult<IUpdater>.BuildFailedCallResult(getUpdaterDefinitionResult, "{0}");

                var loadModuleResult = await this.moduleLoader.LoadModuleAsync(getUpdaterDefinitionResult.Result).ConfigureAwait(false);
                if (!loadModuleResult.Success) return CallResult<IUpdater>.BuildFailedCallResult(loadModuleResult, $"Failed to load updater module for updater with definition id: {updaterDefinitionId}: {{0}}");

                var updaterType = Type.GetType(getUpdaterDefinitionResult.Result.ModuleDescription.TypeFullName);
                if (updaterType == null) return new CallResult<IUpdater>(false, $"Could not recognize updater type: {getUpdaterDefinitionResult.Result.ModuleDescription.TypeFullName}");

                var serviceCollection = new ServiceCollection();  //will probably need to include core application services. have some baseline service collection from ctor 

                serviceCollection.AddTransient(updaterType);    //where does updater service lifetime come from? for now, register as transients 

                foreach (var dependency in getUpdaterDefinitionResult.Result.Dependencies) serviceCollection.TryAdd(dependency);

                if (serviceCollection.BuildServiceProvider().GetRequiredService(updaterType) is not IUpdater updater) 
                    return new CallResult<IUpdater>(false, $"could not inject updater of type: {getUpdaterDefinitionResult.Result.ModuleDescription.TypeFullName}");

                //this.cache.Add(getUpdaterDefinitionResult.Result); //already added from GetUpdaterDefinitionCore() 
                this.cache.Add(updater.Definition.Id, updater);

                return new CallResult<IUpdater>(updater);
            }
            catch (Exception ex) { return CallResult<IUpdater>.FromException(ex); } //handle this 
        }

        public async ValueTask<ICallResult<IUpdaterDefinition>> GetDefinitionAsync(Guid updaterDefinitionId) => await this.GetDefinitionCoreAsync(updaterDefinitionId).ConfigureAwait(false);

        private async ValueTask<ICallResult<IUpdaterDefinition>> GetDefinitionCoreAsync(Guid updaterDefinitionId)
        {
            try
            {
                if (this.cache.TryGetValue(updaterDefinitionId, out IUpdaterDefinition updaterDefinition)) return new CallResult<IUpdaterDefinition>(updaterDefinition);

                var getUpdaterDefinitionResult = await this.definitionProvider.GetAsync(updaterDefinitionId).ConfigureAwait(false);
                if (!getUpdaterDefinitionResult.Success) return getUpdaterDefinitionResult;

                this.cache.Add(getUpdaterDefinitionResult.Result.Id, getUpdaterDefinitionResult.Result);

                return getUpdaterDefinitionResult;
            }
            catch(Exception ex) { return CallResult<IUpdaterDefinition>.FromException(ex); }
        }

        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }
    }
}

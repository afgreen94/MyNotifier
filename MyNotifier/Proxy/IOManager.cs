using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using EventModuleDefinitionModel = MyNotifier.Contracts.EventModules.DefinitionModel;

namespace MyNotifier.Proxy
{
    public abstract class IOManager : IIOManager //should not be abstract class ? //allow method calls to reinitialize?
    {

        protected readonly IFileIOManager fileIOManager;
        protected readonly IConfiguration configuration;
        protected readonly ICallContext<IOManager> callContext;

        protected IFileIOManager.IWrapper fileIOManagerWrapper;
        protected IProxySettings proxySettings;
        protected Paths paths;

        protected bool isInitialized = false;

        protected IOManager(IFileIOManager fileIOManager, IConfiguration configuration, ICallContext<IOManager> callContext)
        {
            this.fileIOManager = fileIOManager;
            this.configuration = configuration;
            this.callContext = callContext;
        }

        public virtual async Task<ICallResult> InitializeAsync(bool forceReinitialize) => await this.InitializeCoreAsync(forceReinitialize).ConfigureAwait(false);

        protected virtual async Task<ICallResult> InitializeCoreAsync(bool forceReinitialize = false)
        {
            throw new NotImplementedException();
        }

        #region Updaters

        public virtual async Task<ICallResult<IUpdaterDefinition>> RetrieveUpdaterDefinitionAsync(Guid updaterDefinitionId)
        {
            if (!this.isInitialized) return new CallResult<IUpdaterDefinition>(false, "Not initialized");

            try
            {
                var retrieveModelResult = await this.RetrieveDefinitionModelAsync<UpdaterDefinitionModel>(this.paths.UpdaterDefinitionsFolder.Path, updaterDefinitionId, "Updater").ConfigureAwait(false);
                if (!retrieveModelResult.Success) return new CallResult<IUpdaterDefinition>(false, retrieveModelResult.ErrorText);

                var updaterDefinition = ModelTranslator.ToUpdaterDefinition(retrieveModelResult.Result);

                return new CallResult<IUpdaterDefinition>(updaterDefinition);
            }
            catch (Exception ex) { return CallResult<IUpdaterDefinition>.FromException(ex); }
        }

        public ICallResult<Stream> CreateUpdaterModuleReadStream(IModuleDescription moduleDescription)
        {
            if (!this.isInitialized) return new CallResult<Stream>(false, "Not initialized.");

            try
            {
                var modulePath = this.fileIOManagerWrapper.BuildAppendedPath(this.paths.DllsFolder.Path, moduleDescription.AssemblyName);

                return this.fileIOManager.CreateReadFileStream(modulePath);

            } catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
        }

        #endregion Updaters

        #region EventModules

        public async Task<ICallResult<IEventModuleDefinition>> RetrieveEventModuleDefinitionAsync(Guid eventModuleDefinitionId)
        {
            if (!this.isInitialized) return new CallResult<IEventModuleDefinition>(false, "Not initialized.");

            try
            {
                var retrieveModelResult = await this.RetrieveDefinitionModelAsync<EventModuleDefinitionModel>(this.paths.EventModuleDefinitionsFolder.Path, eventModuleDefinitionId, "EventModule").ConfigureAwait(false);
                if (!retrieveModelResult.Success) return new CallResult<IEventModuleDefinition>(false, retrieveModelResult.ErrorText);

                var eventModuleDefinition = ModelTranslator.ToEventModuleDefinition(retrieveModelResult.Result);

                return new CallResult<IEventModuleDefinition>(eventModuleDefinition);

            } catch (Exception ex) { return CallResult<IEventModuleDefinition>.FromException(ex); }
        }

        public ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId)
        {
            throw new NotImplementedException();
        }

        #endregion EventModules

        #region Helpers

        protected async Task<ICallResult<TModel>> RetrieveDefinitionModelAsync<TModel>(string path, Guid id, string semanticNamePrefix)
        {
            try
            {
                var filePath = this.fileIOManagerWrapper.BuildAppendedPath(path, id.ToString());

                var createStreamResult = this.fileIOManager.CreateReadFileStream(filePath);
                if (!createStreamResult.Success) return CallResult<TModel>.BuildFailedCallResult(createStreamResult, $"Failed to create read stream for model file of {semanticNamePrefix}Definition with Id: {id}: {{0}}");

                string definitionJson;

                using (var sr = new StreamReader(createStreamResult.Result)) definitionJson = await sr.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(definitionJson)) return new CallResult<TModel>(false, $"Failed to retrieve json for model of {semanticNamePrefix}Definition with Id: {id}: Empty json.");

                var model = JsonSerializer.Deserialize<TModel>(definitionJson);
                if (model == null) return new CallResult<TModel>(false, $"Failed to deserialize json for {semanticNamePrefix}DefinitionModel with Id: {id}: Invalid json.");

                return new CallResult<TModel>(model);

            }
            catch (Exception ex) { return CallResult<TModel>.FromException(ex); }
        }

        //protected static ICallResult<T> BuildFailedToReadFileCallResult<T>(string semanticNamePrefix, Guid id, string errorText = "")
        //{
        //    var errorMessage = $"Failed to read file of {semanticNamePrefix}Definition with Id: {id}";

        //    if (!string.IsNullOrEmpty(errorText)) errorMessage = $"{errorMessage}: {errorText}";

        //    return new CallResult<T>(false, errorMessage);
        //}

        //protected static ICallResult<T> BuildFailedToDeserializeJsonCallResult<T>(string semanticNamePrefix, Guid id) => BuildFailedToReadFileCallResult<T>(semanticNamePrefix, id, "Failed to deserialize json (empty or invalid).");

        #endregion Helpers

        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : IConfiguration
        {
            public IApplicationConfiguration InnerApplicationConfiguration => throw new NotImplementedException();
            public Microsoft.Extensions.Configuration.IConfiguration InnerConfiguration => throw new NotImplementedException();
        }
    }
}

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

namespace MyNotifier.Proxy //namespacing??
{
    public abstract partial class IOManager : IIOManager //should not be abstract class ? //allow method calls to reinitialize? //should not be abstract 
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

        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : IConfiguration
        {
            public IApplicationConfiguration InnerApplicationConfiguration => throw new NotImplementedException();
            public Microsoft.Extensions.Configuration.IConfiguration InnerConfiguration => throw new NotImplementedException();
        }
    }
}

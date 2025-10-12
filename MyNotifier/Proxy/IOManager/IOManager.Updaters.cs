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
    public abstract partial class IOManager : IIOManager
    {
        public virtual async Task<ICallResult<IUpdaterDefinition>> RetrieveUpdaterDefinitionAsync(Guid updaterDefinitionId)
        {
            if (!this.isInitialized) return new CallResult<IUpdaterDefinition>(false, NotInitializedMessage);

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
            if (!this.isInitialized) return new CallResult<Stream>(false, NotInitializedMessage);

            try
            {
                var modulePath = this.fileIOManagerWrapper.BuildAppendedPath(this.paths.DllsFolder.Path, moduleDescription.AssemblyName);

                return this.fileIOManager.CreateReadFileStream(modulePath);

            }
            catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
        }
    }
}

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
        public async Task<ICallResult<IEventModuleDefinition>> RetrieveEventModuleDefinitionAsync(Guid eventModuleDefinitionId)
        {
            if (!this.isInitialized) return new CallResult<IEventModuleDefinition>(false, "Not initialized.");

            try
            {
                var retrieveModelResult = await this.RetrieveDefinitionModelAsync<EventModuleDefinitionModel>(this.paths.EventModuleDefinitionsFolder.Path, eventModuleDefinitionId, "EventModule").ConfigureAwait(false);
                if (!retrieveModelResult.Success) return new CallResult<IEventModuleDefinition>(false, retrieveModelResult.ErrorText);

                var eventModuleDefinition = ModelTranslator.ToEventModuleDefinition(retrieveModelResult.Result);

                return new CallResult<IEventModuleDefinition>(eventModuleDefinition);

            }
            catch (Exception ex) { return CallResult<IEventModuleDefinition>.FromException(ex); }
        }

        public ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId)
        {
            throw new NotImplementedException();
        }
    }
}

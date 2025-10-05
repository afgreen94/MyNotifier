using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using EventModuleDefinitionModel = MyNotifier.Contracts.EventModules.DefinitionModel;

namespace MyNotifier.Proxy.EventModules
{

    public class Provider : IProvider
    {

        private readonly IIOManager ioManager;
        private readonly ICallContext<Provider> callContext;

        public Provider(IIOManager ioManager, ICallContext<Provider> callContext)
        {
            this.ioManager = ioManager;
            this.callContext = callContext;
        }

        public async ValueTask<ICallResult<IEventModuleDefinition>> GetDefinitionAsync(Guid eventModuleDefinitionId)
        {
            try
            {
                var createEventModuleDefinitionStreamResult = this.ioManager.CreateEventModuleStream(eventModuleDefinitionId);
                if (!createEventModuleDefinitionStreamResult.Success) return CallResult<IEventModuleDefinition>.BuildFailedCallResult(createEventModuleDefinitionStreamResult, $"Failed to create read stream for event module definition with id: {eventModuleDefinitionId}: {{0}}");

                string json;
                using (var sr = new StreamReader(createEventModuleDefinitionStreamResult.Result)) json = await sr.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(json)) return new CallResult<IEventModuleDefinition>(false, $"Invalid json for event module definition with id: {eventModuleDefinitionId}");

                var model = JsonSerializer.Deserialize<EventModuleDefinitionModel>(json);
                if (model == null) return new CallResult<IEventModuleDefinition>(false, $"json deserialization failed for event module definition with id: {eventModuleDefinitionId}");

                var definition = ModelTranslator.ToEventModuleDefinition(model);

                return new CallResult<IEventModuleDefinition>(definition);
            }
            catch(Exception ex) { return CallResult<IEventModuleDefinition>.FromException(ex); }
        }

        public ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleId) => throw new NotImplementedException();

    }
}

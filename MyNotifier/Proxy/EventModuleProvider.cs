using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Proxy
{


    public interface IEventModuleProviderProxyIOManager
    {
        ICallResult<Stream> CreateEventModuleDefinitionStream(Guid eventModuleDefinitionId);
        ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId);
    }

    public class EventModuleProvider : IEventModuleProvider
    {

        private readonly IEventModuleProviderProxyIOManager proxyIOManager;
        private readonly ICallContext<EventModuleProvider> callContext;

        public EventModuleProvider(IEventModuleProviderProxyIOManager proxyIOManager, ICallContext<EventModuleProvider> callContext)
        {
            this.proxyIOManager = proxyIOManager;
            this.callContext = callContext;
        }

        public async ValueTask<ICallResult<IEventModuleDefinition>> GetEventModuleDefinitionAsync(Guid eventModuleDefinitionId)
        {
            try
            {
                var createEventModuleDefinitionStreamResult = this.proxyIOManager.CreateEventModuleStream(eventModuleDefinitionId);
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

        public ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleId) => throw new NotImplementedException();
        //{
        //    try
        //    {
        //        var createEventModuleDefinitionStreamResult = this.proxyIOManager.CreateEventModuleStream(eventModuleDefinitionId);
        //        if (!createEventModuleDefinitionStreamResult.Success) return CallResult<IEventModuleDefinition>.BuildFailedCallResult(createEventModuleDefinitionStreamResult, $"Failed to create read stream for event module definition with id: {eventModuleDefinitionId}: {{0}}");


        //    }
        //    catch (Exception ex) { return CallResult<IEventModule>.FromException(ex); }
        //}
    }
}

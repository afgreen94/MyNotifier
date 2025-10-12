using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEventModuleFactory = MyNotifier.Contracts.EventModules.IFactory;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.Interests
{
    public class Factory : MyNotifier.Contracts.Interests.IFactory
    {

        private readonly IEventModuleFactory eventModuleFactory;
        private readonly ICallContext<Factory> callContext;


        public IEventModuleFactory EventModuleFactory => this.eventModuleFactory;


        public Factory(IEventModuleFactory eventModuleFactory, ICallContext<Factory> callContext) { this.eventModuleFactory = eventModuleFactory; this.callContext = callContext; }


        public async ValueTask<ICallResult<IInterest>> GetAsync(Guid[] eventModuleDefinitionIds, IDictionary<Guid, IEventModuleParameterValues[]> parameterValues)
        {
            try
            {
                var eventModules = new List<IEventModule>();

                foreach (var eventModuleDefinitionId in eventModuleDefinitionIds)
                {
                    foreach(var eventModuleParameterValues in parameterValues[eventModuleDefinitionId])
                    {
                        var getEventModuleResult = await this.eventModuleFactory.GetAsync(eventModuleDefinitionId, eventModuleParameterValues).ConfigureAwait(false);
                        if (!getEventModuleResult.Success) return CallResult<IInterest>.BuildFailedCallResult(getEventModuleResult, "Failed to create interest: {0}");

                        eventModules.Add(getEventModuleResult.Result);
                    }
                }
                
                var interest = new Interest() { EventModules = [.. eventModules] };

                return new CallResult<IInterest>(interest);
            }
            catch(Exception ex) { return CallResult<IInterest>.FromException(ex); }
        }

        public ValueTask<ICallResult<IInterest>> GetAsync(InterestModel model)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ICallResult<IInterest>> GetAsync(Contracts.EventModules.IDefinition[] eventModuleDefinitions, IDictionary<Guid, IEventModuleParameterValues[]> parameterValues)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ICallResult<IInterest>> GetAsync(string interestString)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ICallResult<IInterest>> GetAsync(Guid interestId)
        {
            throw new NotImplementedException();
        }

        private ICallResult<IInterest> BuildFailedCallResult() => throw new NotImplementedException();
    }
}

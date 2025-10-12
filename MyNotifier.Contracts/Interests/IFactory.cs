using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using IEventModuleFactory = MyNotifier.Contracts.EventModules.IFactory;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.Contracts.Interests
{
    public interface IFactory : IProvider
    {
        ValueTask<ICallResult<IInterest>> GetAsync(InterestModel model);
        ValueTask<ICallResult> GetAsync(Guid[] eventModuleDefinitionsIds, IDictionary<Guid, IEventModuleParameterValues[]> parameterValues);
        ValueTask<ICallResult<IInterest>> GetAsync(IEventModuleDefinition[] eventModuleDefinitions, IDictionary<Guid, IEventModuleParameterValues[]> parameterValues);
        ValueTask<ICallResult<IInterest>> GetAsync(string interestString);

        IEventModuleFactory EventModuleFactory { get; }
    } 
}

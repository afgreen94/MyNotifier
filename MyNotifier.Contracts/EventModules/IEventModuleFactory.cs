using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.EventModules
{
    public interface IEventModuleFactory : IEventModuleProvider, IEventModuleDefinitionProvider
    {
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(EventModuleModel model);
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(IEventModuleDefinition definition, IEventModuleParameterValues parameterValues);
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleDefinitionId, IEventModuleParameterValues parameterValues); //ParameterValue[][]); //need parameters  
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(string eventModuleString); //parameters included in hash

        IUpdaterFactory UpdaterFactory { get; }
    }
}

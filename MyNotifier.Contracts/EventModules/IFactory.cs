using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterFactory = MyNotifier.Contracts.Updaters.IFactory;

namespace MyNotifier.Contracts.EventModules
{
    public interface IFactory : IProvider, IDefinitionProvider
    {
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(EventModuleModel model);
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(IDefinition definition, IEventModuleParameterValues parameterValues);
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleDefinitionId, IEventModuleParameterValues parameterValues); //ParameterValue[][]); //need parameters  
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(string eventModuleString); //parameters included in hash

        IUpdaterFactory UpdaterFactory { get; }
    }
}

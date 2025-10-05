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
        ValueTask<ICallResult<IEventModule>> GetAsync(EventModuleModel model);
        ValueTask<ICallResult<IEventModule>> GetAsync(IDefinition definition, IParameterValues parameterValues);
        ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleDefinitionId, IParameterValues parameterValues); //ParameterValue[][]); //need parameters  
        ValueTask<ICallResult<IEventModule>> GetAsync(string eventModuleString); //parameters included in hash

        IUpdaterFactory UpdaterFactory { get; }
    }
}

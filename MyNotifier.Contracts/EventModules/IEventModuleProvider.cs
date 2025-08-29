using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.EventModules
{
    public interface IEventModuleDefinitionProvider
    {
        ValueTask<ICallResult<IEventModuleDefinition>> GetEventModuleDefinitionAsync(Guid eventModuleDefinitionId);
    }

    public interface IEventModuleProvider : IEventModuleDefinitionProvider
    {
        ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleId);
    }
}

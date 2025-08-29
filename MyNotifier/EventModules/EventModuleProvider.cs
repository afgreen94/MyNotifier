using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.EventModules;

namespace MyNotifier.EventModules
{
    public abstract class EventModuleProvider : IEventModuleProvider
    {
        public virtual ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleId) => throw new NotImplementedException();
        public virtual ValueTask<ICallResult<IEventModuleDefinition>> GetEventModuleDefinitionAsync(Guid eventModuleDefinitionId) => throw new NotImplementedException();

        protected abstract ValueTask<ICallResult<IEventModuleDefinition>> GetEventModuleDefinitionCoreAsync(Guid eventModuleDefinitionId);
    }
}

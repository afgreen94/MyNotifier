using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.EventModules;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;

namespace MyNotifier.EventModules
{
    public abstract class Provider : IProvider
    {
        public virtual ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleId) => throw new NotImplementedException();
        public virtual ValueTask<ICallResult<IEventModuleDefinition>> GetDefinitionAsync(Guid eventModuleDefinitionId) => throw new NotImplementedException();

        protected abstract ValueTask<ICallResult<IEventModuleDefinition>> GetDefinitionCoreAsync(Guid eventModuleDefinitionId);
    }
}

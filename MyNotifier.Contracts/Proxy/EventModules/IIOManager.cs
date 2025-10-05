using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;

namespace MyNotifier.Contracts.Proxy.EventModules
{
    public interface IIOManager 
    {
        Task<ICallResult<IEventModuleDefinition>> RetrieveEventModuleDefinitionAsync(Guid eventModuleDefinitionId);
        ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId);
    }
}

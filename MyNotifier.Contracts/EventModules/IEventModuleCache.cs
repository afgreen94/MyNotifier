using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.EventModules
{
    public interface IEventModuleCache
    {
        bool TryGetValue(Guid id, out IEventModuleDefinition definition);
        bool TryGetValue(Guid id, out IEventModule eventModule);

        void Add(IEventModuleDefinition definition);
        void Add(IEventModule eventModule);
    }
}

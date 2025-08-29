using MyNotifier.Contracts;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.EventModules
{
    public class EventModuleCache : ICache  //make concurrent ?
    {
        public void Add(IEventModuleDefinition definition)
        {
            throw new NotImplementedException();
        }

        public void Add(IEventModule eventModule)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Guid id, out IEventModuleDefinition definition)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Guid id, out IEventModule eventModule)
        {
            throw new NotImplementedException();
        }
    }
}

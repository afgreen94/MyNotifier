using MyNotifier.Contracts;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Interests;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{
    public class Cache : IInterestCache, IEventModuleCache, IUpdaterCache
    {
        public void Add(IEventModuleDefinition definition)
        {
            throw new NotImplementedException();
        }

        public void Add(IEventModule eventModule)
        {
            throw new NotImplementedException();
        }

        public void Add(IUpdater updater)
        {
            throw new NotImplementedException();
        }

        public void Add(IUpdaterDefinition updaterDefinition)
        {
            throw new NotImplementedException();
        }

        public void Add(IInterest interest)
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

        public bool TryGetValue(Guid id, out IUpdater updater)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Guid id, out IUpdaterDefinition updaterDefinition)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Guid interestId, out IInterest interest)
        {
            throw new NotImplementedException();
        }
    }
}

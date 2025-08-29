using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Updaters
{
    public class UpdaterCache : IUpdaterCache
    {
        public void Add(IUpdater updater)
        {
            throw new NotImplementedException();
        }

        public void Add(IUpdaterDefinition updaterDefinition)
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
    }
}

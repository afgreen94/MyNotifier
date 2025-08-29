using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface IUpdaterCache
    {
        bool TryGetValue(Guid id, out IUpdater updater);
        bool TryGetValue(Guid id, out IUpdaterDefinition updaterDefinition);

        void Add(IUpdater updater);
        void Add(IUpdaterDefinition updaterDefinition);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface ICache : ICache<IUpdater>, ICache<IUpdaterDefinition> { }
}

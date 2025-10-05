using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Updaters
{
    public interface IFactory
    {
        ValueTask<ICallResult<IUpdater>> GetAsync(Guid updaterDefinitionId);
        ValueTask<ICallResult<IUpdaterDefinition>> GetDefinitionAsync(Guid updaterDefinitionId);
    }
}

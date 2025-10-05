using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Updaters
{
    public interface IDefinitionProvider
    {
        ValueTask<ICallResult<IUpdaterDefinition>> GetAsync(Guid updaterDefinitionId);
    }
}

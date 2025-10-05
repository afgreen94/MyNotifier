using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Proxy.Updaters
{
    public interface IIOManager
    {
        Task<ICallResult<IUpdaterDefinition>> RetrieveUpdaterDefinitionAsync(Guid updaterDefinitionId);
        ICallResult<Stream> CreateUpdaterModuleReadStream(IModuleDescription moduleDescription);
    }
}

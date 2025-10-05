using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy.Updaters
{
    public interface IProxyIOManager
    {
        Task<ICallResult<IUpdaterDefinition>> RetrieveUpdaterDefinitionAsync(Guid updaterDefinitionId);
        ICallResult<Stream> CreateModuleReadStream(IUpdaterModuleDescription moduleDescription);
    }
}

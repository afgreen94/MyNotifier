using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Proxy.Updaters
{
    public class UpdaterDefinitionProvider : IUpdaterDefinitionProvider
    {
        private readonly ICallContext<UpdaterDefinitionProvider> callContext;

        public UpdaterDefinitionProvider(ICallContext<UpdaterDefinitionProvider> callContext)
        {
            this.callContext = callContext;
        }

        public ValueTask<ICallResult<IUpdaterDefinition>> GetUpdaterDefinitionAsync(Guid updaterDefinitionId)
        {
            throw new NotImplementedException();
        }
    }
}

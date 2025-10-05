using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy.Updaters;
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
        private readonly IProxyIOManager proxyIOManager;
        private readonly ICallContext<UpdaterDefinitionProvider> callContext;

        public UpdaterDefinitionProvider(IProxyIOManager proxyIOManager, ICallContext<UpdaterDefinitionProvider> callContext)
        {
            this.proxyIOManager = proxyIOManager;
            this.callContext = callContext;
        }

        public async ValueTask<ICallResult<IUpdaterDefinition>> GetUpdaterDefinitionAsync(Guid updaterDefinitionId)
        {
            try
            {
                var retrieveUpdaterDefinitionResult = await this.proxyIOManager.RetrieveUpdaterDefinitionAsync(updaterDefinitionId);
                if(retrieveUpdaterDefinitionResult.Success) return CallResult<IUpdaterDefinition>.BuildFailedCallResult(retrieveUpdaterDefinitionResult, $"Failed to retrieve updater definition with Id: {updaterDefinitionId}: {{0}}");

                return new CallResult<IUpdaterDefinition>(retrieveUpdaterDefinitionResult.Result);
            }
            catch (Exception ex) { return CallResult<IUpdaterDefinition>.FromException(ex); }
        }
    }
}

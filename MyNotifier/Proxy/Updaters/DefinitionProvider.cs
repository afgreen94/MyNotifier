using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy.Updaters;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Proxy.Updaters
{
    public class DefinitionProvider : IDefinitionProvider
    {
        private readonly IIOManager ioManager;
        private readonly ICallContext<DefinitionProvider> callContext;

        public DefinitionProvider(IIOManager ioManager, ICallContext<DefinitionProvider> callContext)
        {
            this.ioManager = ioManager;
            this.callContext = callContext;
        }

        public async ValueTask<ICallResult<IUpdaterDefinition>> GetAsync(Guid updaterDefinitionId)
        {
            try
            {
                var retrieveUpdaterDefinitionResult = await this.ioManager.RetrieveUpdaterDefinitionAsync(updaterDefinitionId);
                if(retrieveUpdaterDefinitionResult.Success) return CallResult<IUpdaterDefinition>.BuildFailedCallResult(retrieveUpdaterDefinitionResult, $"Failed to retrieve updater definition with Id: {updaterDefinitionId}: {{0}}");

                return new CallResult<IUpdaterDefinition>(retrieveUpdaterDefinitionResult.Result);
            }
            catch (Exception ex) { return CallResult<IUpdaterDefinition>.FromException(ex); }
        }
    }
}

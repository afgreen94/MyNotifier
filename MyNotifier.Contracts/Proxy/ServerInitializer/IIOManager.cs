using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;

namespace MyNotifier.Contracts.Proxy.ServerInitializer
{
    public interface IIOManager
    {
        ValueTask<ICallResult> EnsureProxyFileSystemAsync();
        ValueTask<ICallResult<ICatalog>> LoadCatalogAsync();
        ValueTask<ICallResult<InterestModel[]>> LoadInterestModelsAsync();
        ValueTask<ICallResult<HashSet<UpdaterDefinitionModel>>> ValidateAndInitializeForKnownUpdaterDefinitionsAsync(InterestModel[] interests, ICatalog catalog);
    }
}

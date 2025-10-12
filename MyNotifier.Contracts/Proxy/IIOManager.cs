using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy
{
    public interface IIOManager : ServerInitializer.IIOManager, 
                                  Updaters.IIOManager,
                                  EventModules.IIOManager, 
                                  Publishers.IIOManager, 
                                  Notifiers.IIOManager 
                                  /*, IFileIOManager !!!See notes in Proxy/IOManager.FileIOManager */
    {
        IProxySettings ProxySettings { get; } //maybe should have dedicated Set Method ? //idk maybe should not be exposed 
        Task<ICallResult> InitializeAsync(IProxySettings proxySettings, bool forceReInitialize = false);
    }
}

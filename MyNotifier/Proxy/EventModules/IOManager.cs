using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Proxy.EventModules
{
    //public interface IIOManager
    //{
    //    Task<ICallResult<IEventModuleDefinition>> RetrieveEventModuleDefinitionAsync(Guid eventModuleDefinitionId);
    //    ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId);
    //}

    //public class IOManager : Proxy.IOManager, IIOManager
    //{

    //    private readonly IFileIOManager fileIOManager; 
    //    private readonly ICallContext<IOManager> callContext;

    //    public IOManager(IFileIOManager fileIOManager, IConfiguration configuration, ICallContext<Proxy.IOManager> callContext) : base(fileIOManager, configuration, callContext)
    //    {
    //    }

    //    private 
    //    public Task<ICallResult<IEventModuleDefinition>> RetrieveEventModuleDefinitionAsync(Guid eventModuleDefinitionId)
    //    {
    //        throw new NotImplementedException();

    //    }

    //    public ICallResult<Stream> CreateEventModuleStream(Guid eventModuleId)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

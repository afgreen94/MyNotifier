using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.EventModules
{
    public interface IDefinitionProvider
    {
        ValueTask<ICallResult<IDefinition>> GetDefinitionAsync(Guid eventModuleDefinitionId);
    }

    public interface IProvider : IDefinitionProvider
    {
        ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleId);
    }
}

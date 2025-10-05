using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using IEventModuleFactory = MyNotifier.Contracts.EventModules.IFactory;

namespace MyNotifier.Contracts.Interests
{
    public interface IInterestFactory : IInterestProvider
    {
        ValueTask<ICallResult<IInterest>> GetInterestAsync(InterestModel model);
        ValueTask<ICallResult<IInterest>> GetInterestAsync(IEventModuleDefinition eventModuleDefinition, IDictionary<Guid, IEventModuleParameterValues> parameterValues);
        ValueTask<ICallResult<IInterest>> GetInterestAsync(string interestString);

        IEventModuleFactory EventModuleFactory { get; }
    } 
}

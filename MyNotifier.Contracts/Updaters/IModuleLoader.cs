using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts.Updaters
{
    public interface IModuleLoader  //should only need updaterModuleDescriptions, rather than entire definition 
    {
        ValueTask<IResult> LoadModuleAsync(IUpdaterDefinition updaterDefinition);
        IAsyncEnumerable<IResult> LoadModulesAsync(params IUpdaterDefinition[] updaterDefinitions);

        public interface IResult : ICallResult<IUpdaterDefinition> { }
    }
}

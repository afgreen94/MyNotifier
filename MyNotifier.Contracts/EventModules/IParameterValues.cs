using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.EventModules
{
    public interface IParameterValues
    {
        IReadOnlyDictionary<Guid, ParameterValue[]> UpdaterParameters { get; } //<Guid, ParameterValues[][]>, multiple parameters sets for same IDefinition, implement this !!! 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface IUpdaterModuleDescription
    {
        //Type Type { get; }
        string AssemblyName { get; }
        string TypeFullName { get; }
        string DefinitionTypeFullName { get; }

        //IUpdaterDefinition ? //one contains the other ? 
    }

    public class UpdaterModuleDescription : IUpdaterModuleDescription
    {
        //public Type Type { get; set; }
        public string AssemblyName { get; set; }
        public string TypeFullName { get; set; }
        public string DefinitionTypeFullName { get; set; }
    }
}

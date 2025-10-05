using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Updaters
{
    public interface IModuleDescription : Base.IDefinition
    {
        //Type Type { get; }
        string AssemblyName { get; }
        string TypeFullName { get; }
        string DefinitionTypeFullName { get; }

        //IUpdaterDefinition ? //one contains the other ? 
    }

    public class ModuleDescription : IModuleDescription
    {
        public Guid Id { get; set;  }
        public string Name { get; set;  }
        public string Description { get; set; }

        //public Type Type { get; set; }
        public string AssemblyName { get; set; }
        public string TypeFullName { get; set; }
        public string DefinitionTypeFullName { get; set; }
    }
}

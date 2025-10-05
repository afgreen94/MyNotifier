using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MyNotifier.Contracts.Updaters
{
    public interface IDefinition : MyNotifier.Contracts.Base.IDefinition
    {
        IParameterDefinition[] ParameterDefinitions { get; }
        IModuleDescription ModuleDescription { get; } // for now, definition includes module description, could decouple later 
        HashSet<ServiceDescriptor> Dependencies { get; }
    }

    public class CustomDefinition : Definition, IDefinition
    {
        public IParameterDefinition[] ParameterDefinitions { get; set; }
        public IModuleDescription ModuleDescription { get; set;  }
        public HashSet<ServiceDescriptor> Dependencies { get; set; }
    }

    public class DefinitionModel : Definition, MyNotifier.Contracts.Base.IDefinition
    {
        public ParameterDefinition[] ParameterDefinitions { get; set; }
        public ModuleDescription ModuleDescription { get; set; }
        public HashSet<ServiceDescriptor> Dependencies { get; set; }
    }
}

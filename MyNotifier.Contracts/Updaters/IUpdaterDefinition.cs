using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MyNotifier.Contracts.Updaters
{
    public interface IUpdaterDefinition : IDefinition
    {
        IParameterDefinition[] ParameterDefinitions { get; }
        IUpdaterModuleDescription ModuleDescription { get; } // for now, definition includes module description, could decouple later 
        HashSet<ServiceDescriptor> Dependencies { get; }
    }

    public class CustomUpdaterDefinition : Definition, IUpdaterDefinition
    {
        public IParameterDefinition[] ParameterDefinitions { get; set; }
        public IUpdaterModuleDescription ModuleDescription { get; set;  }
        public HashSet<ServiceDescriptor> Dependencies { get; set; }
    }

    public class UpdaterDefinitionModel : Definition, IDefinition
    {
        public ParameterDefinition[] ParameterDefinitions { get; set; }
        public UpdaterModuleDescription ModuleDescription { get; set; }
        public HashSet<ServiceDescriptor> Dependencies { get; set; }
    }
}

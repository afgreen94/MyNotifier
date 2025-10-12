using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Updaters;

namespace MyNotifier.Contracts.EventModules
{
    public interface IEventModule
    {
        IDefinition Definition { get; }
        IDictionary<Guid, UpdaterParametersWrapper> UpdaterParameterWrappers { get; }
    }

    public interface IDescription
    {
        IDefinition Definition { get; }
        IDictionary<Guid, Parameter[]> Parameters { get; }
    }

    public class DescriptionModel
    {
        public DefinitionModel Definition { get; set; }
        public Dictionary<Guid, Parameter[]> Parameters { get; set; }
    }

    public class CustomDescription : IDescription
    {
        public IDefinition Definition { get; set; }
        public IDictionary<Guid, Parameter[]> Parameters { get; set; }
    }

    public abstract class EventModuleBase : IEventModule
    {
        public abstract IDefinition Definition { get; }
        public abstract IDictionary<Guid, UpdaterParametersWrapper> UpdaterParameterWrappers { get; }
    }

    public class CustomEventModule : IEventModule
    {
        public IDefinition Definition { get; set; }
        public IDictionary<Guid, UpdaterParametersWrapper> UpdaterParameterWrappers { get; set; } = new Dictionary<Guid, UpdaterParametersWrapper>();
    }

    public class EventModuleModel
    {
        public DefinitionModel Definition { get; set; }
        public Dictionary<Guid, Parameter[]> Parameters { get; set; } //EventModuleParameterValuesModel <-- //should be 2D parameter matrix 
    }
}

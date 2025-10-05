using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;

namespace MyNotifier.Contracts.EventModules
{
    public interface IDefinition : Base.IDefinition
    {
        IUpdaterDefinition[] UpdaterDefinitions { get; }
        //IUpdaterSet CommonUpdaterDefinitions { get; }
    }

    public abstract class DefinitionBase : IDefinition //type of event
    {
        public abstract Guid Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract IUpdaterDefinition[] UpdaterDefinitions { get; }
    }

    public class CustomDefinition : IDefinition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IUpdaterDefinition[] UpdaterDefinitions { get; set; }
    }

    public class DefinitionModel : Definition
    {
        public UpdaterDefinitionModel[] UpdaterDefinitions { get; set; }
    }

}

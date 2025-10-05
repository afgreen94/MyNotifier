using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MyNotifier.Updaters
{
    public abstract class Definition : MyNotifier.Contracts.Updaters.IDefinition //also in Updater.cs, will figure this out later 
    {
        public abstract Guid Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }

        protected IParameterDefinition[] parameterDefinitions;

        public virtual IParameterDefinition[] ParameterDefinitions
        {
            get
            {
                if (this.parameterDefinitions != null) return this.parameterDefinitions;

                var parameterDefinitions = this.GetParameterDefinitionsCore();
                this.parameterDefinitions = new ParameterDefinition[parameterDefinitions.Length + 1];
                this.parameterDefinitions[0] = Contracts.ParameterDefinitions.Base.NotificationReturnProtocol();
                for (int i = 1; i < this.parameterDefinitions.Length; i++) this.parameterDefinitions[i] = parameterDefinitions[i - 1];

                return this.parameterDefinitions;
            }
        }

        public abstract IModuleDescription ModuleDescription { get; }
        protected abstract IParameterDefinition[] GetParameterDefinitionsCore();

        //dependencies? 

        public string AssemblyName { get; }
        //all are non-qualified names
        public string TypeFullName { get; }
        public string DefinitionTypeFullName { get; }

        public abstract HashSet<ServiceDescriptor> Dependencies { get; }
    }
}

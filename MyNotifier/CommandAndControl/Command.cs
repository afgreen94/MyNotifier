using MyNotifier.Contracts;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    public abstract class Command : ICommand
    {
        public ICommandDefinition Definition => throw new NotImplementedException();
        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public abstract class InterestModelsCommand : Command, IInterestModelCommand { public virtual InterestModel[] InterestModels { get; set; } }
    //public abstract class InterestDefinitionIdsCommand : Command, IInterestDefinitionIdsCommand { public virtual Guid[] InterestDefinitionIds { get; set; } }
    public abstract class ApplicationConfigurationCommand : Command, IApplicationConfigurationCommand { public virtual IApplicationConfiguration ApplicationConfiguration { get; set; } }




    //update interest with event module(s)
        //add event module(s) to interest 
        //remove event module(s) from interest
}

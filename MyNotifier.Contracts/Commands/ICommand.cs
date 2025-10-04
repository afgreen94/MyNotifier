using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Commands
{
    public interface ICommand 
    {
        ICommandDefinition Definition { get; }
        Parameter[] Parameters { get; }
    }

    public interface IInterestModelCommand : ICommand { InterestModel[] InterestModels { get; } }
    //public interface IInterestDefinitionIdsCommand : ICommand { Guid[] InterestDefinitionIds { get; } }
    public interface IApplicationConfigurationCommand : ICommand { IApplicationConfiguration ApplicationConfiguration { get; } }

    public interface IRegisterAndSubscribeToNewInterests : IInterestModelCommand { }
    public interface ISubscribeToInterestsByDefinitionIds : ICommand //IInterestDefinitionIdsCommand { }
    {
        Guid InterestDefinitionId { get; }
        ParameterValue[] ParameterValues { get; }
    }
    public interface IUnsubscribeFromInterests : IInterestModelCommand { }
    public interface IChangeApplicationConfiguration : IApplicationConfigurationCommand { }

}

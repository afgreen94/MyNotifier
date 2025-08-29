using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Commands;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Commands
{
    public abstract class Command : ICommand { public abstract IDefinition Definition { get; } }

    public abstract class InterestModelsCommand : Command, IInterestModelCommand { public virtual InterestModel[] InterestModels { get; set; } }
    //public abstract class InterestDefinitionIdsCommand : Command, IInterestDefinitionIdsCommand { public virtual Guid[] InterestDefinitionIds { get; set; } }
    public abstract class ApplicationConfigurationCommand : Command, IApplicationConfigurationCommand { public virtual IApplicationConfiguration ApplicationConfiguration { get; set; } }


    public class RegisterAndSubscribeToNewInterests : InterestModelsCommand, IRegisterAndSubscribeToNewInterests
    {
        private static readonly Definition definition = new()
        {
            Id = new("{}"),
            Name = nameof(RegisterAndSubscribeToNewInterests),
            Description = "Registers (and subscribes to?) new interest(s)."
        };
        public override IDefinition Definition => definition;
    }
    public class SubscribeToInterestsByDefinitionIds : Command, ISubscribeToInterestsByDefinitionIds
    {
        private static readonly Definition definition = new()
        {
            Id = new("{}"),
            Name = nameof(SubscribeToInterestsByDefinitionIds),
            Description = "Subscribes to existing interest(s) by interest definition id(s)"
        };
        public override IDefinition Definition => definition;

        public Guid InterestDefinitionId { get; set; }
        public ParameterValue[] ParameterValues { get; set; }
    }
    public class UnsubscribeFromInterests : InterestModelsCommand, IUnsubscribeFromInterests
    {
        private static readonly Definition definition = new()
        {
            Id = new("{}"),
            Name = nameof(UnsubscribeFromInterests),
            Description = "Unsubscribers from interest(s)."
        };
        public override IDefinition Definition => definition;
    }
    public class ChangeApplicationConfiguration : ApplicationConfigurationCommand, IChangeApplicationConfiguration
    {
        private static readonly Definition definition = new()
        {
            Id = new("{}"),
            Name = nameof(ChangeApplicationConfiguration),
            Description = "Updates application configuration."
        };
        public override IDefinition Definition => definition;
    }


    //add event module to interest 
}

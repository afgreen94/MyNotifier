using MyNotifier.Contracts.Base;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.CommandAndControl.Commands;

namespace MyNotifier.CommandAndControl.Commands
{
    public abstract class Command : ICommand 
    {
        public ICommandDefinition Definition => throw new NotImplementedException();
        public Parameter[] Parameters => throw new NotImplementedException();
    }

    public abstract class InterestModelsCommand : Command, IInterestModelCommand { public virtual InterestModel[] InterestModels { get; set; } }
    //public abstract class InterestDefinitionIdsCommand : Command, IInterestDefinitionIdsCommand { public virtual Guid[] InterestDefinitionIds { get; set; } }
    public abstract class ApplicationConfigurationCommand : Command, IApplicationConfigurationCommand { public virtual IApplicationConfiguration ApplicationConfiguration { get; set; } }


    public interface IRegisterAndSubscribeToNewInterestsDefinition : ICommandDefinition { }
    public class RegisterAndSubscribeToNewInterestsDefinition : IRegisterAndSubscribeToNewInterestsDefinition
    {
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Type FactoryType => throw new NotImplementedException();

        public Type CommandType => throw new NotImplementedException();

        public Type ServiceType => throw new NotImplementedException();
    }
    //public class RegisterAndSubscribeToNewInterests : InterestModelsCommand, IRegisterAndSubscribeToNewInterests
    //{
    //    private static readonly Definition definition = new()
    //    {
    //        Id = new("{}"),
    //        Name = nameof(RegisterAndSubscribeToNewInterests),
    //        Description = "Registers (and subscribes to?) new interest(s)."
    //    };
    //    public override IDefinition Definition => definition;
    //}

    public interface ISubscribeToInterestsByIdDefinition : ICommandDefinition { }
    public class SubscribeToInterestsByIdsDefinition : ISubscribeToInterestsByIdDefinition
    {
        public Type FactoryType => throw new NotImplementedException();

        public Type CommandType => throw new NotImplementedException();

        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Type ServiceType => throw new NotImplementedException();
    }
    //public class SubscribeToInterestsByDefinitionIds : Command, ISubscribeToInterestsByDefinitionIds
    //{
    //    private static readonly Definition definition = new()
    //    {
    //        Id = new("{}"),
    //        Name = nameof(SubscribeToInterestsByDefinitionIds),
    //        Description = "Subscribes to existing interest(s) by interest definition id(s)"
    //    };
    //    public override IDefinition Definition => definition;

    //    public Guid InterestDefinitionId { get; set; }
    //    public ParameterValue[] ParameterValues { get; set; }
    //}

    public interface IUnsubscribeFromInterestsByIdDefinition : ICommandDefinition { }
    public class UnsubscribeFromInterestsByIdDefinition : IUnsubscribeFromInterestsByIdDefinition
    {
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Type FactoryType => throw new NotImplementedException();

        public Type CommandType => throw new NotImplementedException();

        public Type ServiceType => throw new NotImplementedException();
    }
    //public class UnsubscribeFromInterests : InterestModelsCommand, IUnsubscribeFromInterests
    //{
    //    private static readonly Definition definition = new()
    //    {
    //        Id = new("{}"),
    //        Name = nameof(UnsubscribeFromInterests),
    //        Description = "Unsubscribers from interest(s)."
    //    };
    //    public override IDefinition Definition => definition;
    //}

    public interface IChangeApplicationConfigurationDefinition : ICommandDefinition { }
    public class ChangeApplicationConfigurationDefinition : IChangeApplicationConfigurationDefinition
    {
        public IParameterDefinition[] ParameterDefinitions => throw new NotImplementedException();

        public Guid Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public Type FactoryType => throw new NotImplementedException();

        public Type CommandType => throw new NotImplementedException();

        public Type ServiceType => throw new NotImplementedException();
    }
    //public class ChangeApplicationConfiguration : ApplicationConfigurationCommand, IChangeApplicationConfiguration
    //{
    //    private static readonly Definition definition = new()
    //    {
    //        Id = new("{}"),
    //        Name = nameof(ChangeApplicationConfiguration),
    //        Description = "Updates application configuration."
    //    };
    //    public override IDefinition Definition => definition;
    //}


    //add event module to interest 
}

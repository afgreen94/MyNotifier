using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;
using CustomUpdaterDefinition = MyNotifier.Contracts.Updaters.CustomDefinition;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using EventModuleDefinitionModel = MyNotifier.Contracts.EventModules.DefinitionModel;
using CustomEventModuleDefinition = MyNotifier.Contracts.EventModules.CustomDefinition;

namespace MyNotifier
{
    public class ModelTranslator
    {

        public static IInterestDefinition ToInterestDefinition(InterestDefinitionModel model) => ToInterestDefinitionCore(model);
        public static InterestDefinitionModel ToModel(IInterestDefinition definition) => ToModel(definition);

        public static IInterest ToInterest(InterestModel interestModel) => ToInterestCore(interestModel);
        public static InterestModel ToModel(IInterest interest) => ToModelCore(interest);

        public static IEventModuleDefinition ToEventModuleDefinition(EventModuleDefinitionModel model) => ToEventModuleDefinitionCore(model);
        public static EventModuleDefinitionModel ToModel(IEventModuleDefinition definition) => ToModelCore(definition);

        //public static IEventModule ToEventModule(EventModuleModel model) => ToEventModuleCore(model);
        public static EventModuleModel ToModel(IEventModule eventModule) => ToModelCore(eventModule);

        public static IUpdaterDefinition ToUpdaterDefinition(UpdaterDefinitionModel model) => ToUpdaterDefinitionCore(model);
        public static UpdaterDefinitionModel ToModel(IUpdaterDefinition definition) => ToModelCore(definition);

        private static IInterestDefinition ToInterestDefinitionCore(InterestDefinitionModel model) => throw new NotImplementedException();
        private static InterestDefinitionModel ToModelCore(IInterestDefinition definition) => throw new NotImplementedException();


        private static IInterest ToInterestCore(InterestModel interestModel) => throw new NotImplementedException();

        private static InterestModel ToModelCore(IInterest interest)
        {
            var model = new InterestModel()
            {
                Definition = new Definition()
                {
                    Id = interest.Definition.Id,
                    Name = interest.Definition.Name,
                    Description = interest.Definition.Description
                },
                EventModuleModels = new EventModuleModel[interest.EventModules.Length]
            };

            for (int i = 0; i < model.EventModuleModels.Length; i++) model.EventModuleModels[i] = ToModel(interest.EventModules[i]);

            return model;
        }


        //private static IEventModule ToEventModuleCore(EventModuleModel model) { }

        private static EventModuleModel ToModelCore(IEventModule eventModule)
        {
            var model = new EventModuleModel()
            {
                Definition = ToModelCore(eventModule.Definition),
                Parameters = []
            };

            foreach (var wrapper in eventModule.UpdaterParameterWrappers) model.Parameters[wrapper.Key] = wrapper.Value.Parameters;

            return model;
        }

        private static IEventModuleDefinition ToEventModuleDefinitionCore(EventModuleDefinitionModel model)
        {
            var definition = new CustomEventModuleDefinition()
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                UpdaterDefinitions = new CustomUpdaterDefinition[model.UpdaterDefinitions.Length]
            };

            for(int i = 0; i < definition.UpdaterDefinitions.Length; i++) definition.UpdaterDefinitions[i] = ToUpdaterDefinition(model.UpdaterDefinitions[i]);

            return definition;
        }

        private static EventModuleDefinitionModel ToModelCore(IEventModuleDefinition definition)
        {
            var model = new EventModuleDefinitionModel()
            {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                UpdaterDefinitions = new UpdaterDefinitionModel[definition.UpdaterDefinitions.Length]
            };

            for(int i = 0; i < model.UpdaterDefinitions.Length; i++) model.UpdaterDefinitions[i] = ToModel(definition.UpdaterDefinitions[i]);

            return model;
        }


        private static IUpdaterDefinition ToUpdaterDefinitionCore(UpdaterDefinitionModel model) => new CustomUpdaterDefinition()
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Dependencies = model.Dependencies,
            ModuleDescription = model.ModuleDescription,
            ParameterDefinitions = model.ParameterDefinitions
        };

        private static UpdaterDefinitionModel ToModelCore(IUpdaterDefinition definition) //make copies of ref types ?
        {

            var model = new UpdaterDefinitionModel
            {
                Id = definition.Id,
                Name = definition.Name,
                Description = definition.Description,
                Dependencies = definition.Dependencies,
                ModuleDescription = new ModuleDescription()
                {
                    AssemblyName = definition.ModuleDescription.AssemblyName,
                    TypeFullName = definition.ModuleDescription.TypeFullName,
                    DefinitionTypeFullName = definition.ModuleDescription.DefinitionTypeFullName
                },
                ParameterDefinitions = new ParameterDefinition[definition.ParameterDefinitions.Length]
            };

            for (int i = 0; i < model.ParameterDefinitions.Length; i++)
            {
                model.ParameterDefinitions[i] = new ParameterDefinition()
                {
                    Id = definition.ParameterDefinitions[i].Id,
                    Name = definition.ParameterDefinitions[i].Name,
                    Description = definition.ParameterDefinitions[i].Description,
                    Type = definition.ParameterDefinitions[i].Type,
                    IsRequired = definition.ParameterDefinitions[i].IsRequired
                };
            }

            return model;
        }

        //private static void SetDefinitionProperties(IDefinition fromDefinition, Definition toDefinition)
        //{
        //    toDefinition.Id = fromDefinition.Id;
        //    toDefinition.Name = fromDefinition.Name;
        //    toDefinition.Description = fromDefinition.Description;
        //}
    }
}

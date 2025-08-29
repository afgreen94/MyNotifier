using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;
using Google.Apis.Requests.Parameters;
using MyNotifier.Updaters;
using MyNotifier.Contracts.Base;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace MyNotifier.Test
{
    public class Experiments
    {







        [Fact]
        public async Task Test3()
        {


            Guid interestDefinitionId = default; 
            ParameterValue[][][] parameterValues = default;

            InterestModel interestModel = default;
            string interestHash = default;

            IInterestFactory interestFactory = default;

            var interestByDefinitionIdAndParameterValues = await interestFactory.GetInterestAsync(interestDefinitionId, parameterValues);
            var interestByInterestModel = await interestFactory.GetInterestAsync(interestModel);
            var interestByHash = await interestFactory.GetInterestAsync(interestHash);


            Guid eventModuleDefinitionId = default;
            ParameterValue[][] parameterValues1 = default;

            EventModuleModel model1 = default;
            string eventModuleHash = default;

            IEventModuleFactory eventModuleFactory = default;

            var eventModuleByDefinitionIdAndParameterValues = await eventModuleFactory.GetEventModuleAsync(eventModuleDefinitionId, parameterValues1);
            var eventModuleByModel = await eventModuleFactory.GetEventModuleAsync(model1);
            var eventModuleByHash = await eventModuleFactory.GetEventModuleAsync(eventModuleHash);


            Guid updaterDefinitionId = default;
            ParameterValue[] parameterValues2 = default;

            UpdaterDefinitionModel = default;
            string updaterHash = default;

            IUpdaterFactory updaterFactory = default;







        }






















        private class DefinitionConverter : JsonConverter<IDefinition>
        {
            public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableFrom(typeof(IDefinition));
            public override IDefinition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (IDefinition)JsonSerializer.Deserialize(reader: ref reader, typeof(Definition));

            public override void Write(Utf8JsonWriter writer, IDefinition value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task Test1()
        {
            string json;
            using (var sr = new StreamReader("test.json")) json = await sr.ReadToEndAsync();

            var definition = JsonSerializer.Deserialize<IDefinition>(json, new JsonSerializerOptions()
            {

            });
        }

        [Fact]
        public async Task Test0()
        {

            //"Interest" -> A class of events [Collection of EventModules]
            //InterestDefinition -> defines type of interest [class of types of events]
            //interest -> instance of InterestDefinition [class of events/collection of event modules]
            //interestDescription -> describes specific instance 

            //"Event Module" -> a collection of updaters 
            //EventModuleDefinition -> defines type of eventModule
            //EventModule -> instance of EventModuleDefinition [updaters w/ parameters]
            //EventModuleDescription -> describes instance

            //updater -> scans for events 
            //updater definition -> type of updater [including parameter definitions] //abstractions, must be hardcoded for reasons given below @ {note 1.0}
            //updater -> instance of updaterDefinition, given parameters foreach updaterDefinition parameter definitions 

            //"base" updater parameters -> common, ie return protocols, etc
            //"generic" -> specific to updater type 



            var cf_eventModuleDefinition = new CustomEventModuleDefinition()  //maybe updater module has a way to route common parameters to updaters
            {
                Id = new("{}"),
                Name = "CF Event Module Definition",
                UpdaterDefinitions = 
                [
                    new WF_UpdaterDefinition(), 
                    new R_CF_UpdaterDefinition(), 
                    new R_CS_UpdaterDefition()
                ]
            };

            string description = "";
            IDictionary<Guid, ParameterValue[]> parameters = new Dictionary<Guid, ParameterValue[]>();
            IUpdaterFactory updaterFactory = new UpdaterFactory(null, null);

            new EventModuleHelper().TryBuildEventModule(cf_eventModuleDefinition,
                                                        description,
                                                        parameters,
                                                        updaterFactory,
                                                        out var eventModule,
                                                        out var _);

            foreach (var updater in eventModule.Updaters)
            {
                var updaterParameters = eventModule.Parameters[updater.Definition.Id];
                await updater.InitializeAsync();

                var getUpdateResult = await updater.TryGetUpdateAsync(updaterParameters);
            }
        }

        //{note 1.0}
        //updater definitions are abstract because an updater definition only comes into being when a new updater is written. By design, definitions will be created and wrapped in with updaters as they are written
        //other definition objects can be dynamically built. this functionality must be implemented. for now they are all abstractions 
        //definition objects are abstract, meaning specific values must correspond to hardcoded type
        //will need customizeable derivative type, protected type and use fluid-building to issue instances as implementations of corresponding interface 

        //parameter-common updater definitions ? all udefns share same parameterValue for same parameter defn 

        public class WF_UpdaterDefinition : UpdaterDefinition
        {
            private readonly IParameterDefinition[] _parameterDefinitions = [Contracts.ParameterDefinitions.Common.Name(true)];
            private readonly IUpdaterModuleDescription _updaterModuleDescription = new UpdaterModuleDescription();


            public override Guid Id => new("{}");
            public override string Name => "WF_UpdaterDefinition";
            public override string Description => "Watches for new WF postings for {pd:Name}";

            protected override IParameterDefinition[] GetParameterDefinitionsCore() => this._parameterDefinitions;
            public override IUpdaterModuleDescription ModuleDescription => this._updaterModuleDescription;

            public override HashSet<ServiceDescriptor> Dependencies => throw new NotImplementedException();

            protected class UpdaterModuleDescription : Contracts.Updaters.UpdaterModuleDescription
            {
                public override string AssemblyName => "";
                public override string TypeFullName => "";
                public override string DefinitionTypeFullName => "";
            }
        }

        public class R_CF_UpdaterDefinition : UpdaterDefinition
        {
            private readonly IParameterDefinition[] _parameterDefinitions = [Contracts.ParameterDefinitions.Common.Name(true)];
            private readonly IUpdaterModuleDescription _updaterModuleDescription = new UpdaterModuleDescription();


            public override Guid Id => new("{}");
            public override string Name => "R_CF_UpdaterDefinition";
            public override string Description => "Watches for new R_CF postings for {pd:Name}";

            protected override IParameterDefinition[] GetParameterDefinitionsCore() => this._parameterDefinitions;
            public override IUpdaterModuleDescription ModuleDescription => this._updaterModuleDescription;


            protected class UpdaterModuleDescription : Contracts.Updaters.UpdaterModuleDescription
            {
                public override string AssemblyName => "";
                public override string TypeFullName => "";
                public override string DefinitionTypeFullName => "";
            }
        }

        public class R_CS_UpdaterDefition : UpdaterDefinition
        {
            private readonly IParameterDefinition[] _parameterDefinitions = [Contracts.ParameterDefinitions.Common.Name(true)];
            private readonly IUpdaterModuleDescription _updaterModuleDescription = new UpdaterModuleDescription();


            public override Guid Id => new("{}");
            public override string Name => "R_CS_UpdaterDefinition";
            public override string Description => "Watches for new R_CS postings for {pd:Name}";


            protected override IParameterDefinition[] GetParameterDefinitionsCore() => this._parameterDefinitions;
            public override IUpdaterModuleDescription ModuleDescription => this._updaterModuleDescription;


            protected class UpdaterModuleDescription : Contracts.Updaters.UpdaterModuleDescription
            {
                public override string AssemblyName => "";
                public override string TypeFullName => "";
                public override string DefinitionTypeFullName => "";
            }
        }

        public class CF_EventModuleDefinition : EventModuleDefinitionBase
        {
            public override Guid Id => definition.Id;
            public override string Name => definition.Name;
            public override string Description => definition.Description;
            public override IUpdaterDefinition[] UpdaterDefinitions => updaterDefinitions;


            protected static readonly Definition definition = new()
            {
                Id = new("{}"),
                Name = "CF Event Module Definition",
                Description = "Watches for CF for {pd:Name} via updaterDefinition[] channels"
            };
            protected static readonly IUpdaterDefinition wf_updaterDefinition = new WF_UpdaterDefinition();
            protected static readonly IUpdaterDefinition r_cf_updaterDefinition = new R_CF_UpdaterDefinition();
            protected static readonly IUpdaterDefinition[] updaterDefinitions = [wf_updaterDefinition, r_cf_updaterDefinition];
        }

        ////maybe at this level, interests should be built dynamically 
        //public class CF_InterestDefinition : Interest0Definition
        //{
        //    protected static readonly Definition definition = new()
        //    {
        //        Id = new("{}"),
        //        Name = "CF Interest Definition",
        //        Description = "Watches for CF. 1 EventModuleDefinition per 1 {pd:Name} via channels defined in EventModuleDefinitions"
        //    };

        //    protected static readonly EventModuleDefinitionBase eventModuleDefinition = new CF_EventModuleDefinition();

        //    public override Definition Definition => definition;
        //    public override EventModuleDefinition[] EventModuleDefinitions => throw new NotImplementedException();
        //}

        //public abstract class Interest0Definition //type of class of events 
        //{
        //    public abstract Definition Definition { get; }
        //    public abstract EventModuleDefinition[] EventModuleDefinitions { get; }
        //}

        //public class Interest0 //class of events 
        //{
        //    public Interest0Definition Definition { get; set; }
        //    public string Description { get; set; }

        //    public EventModule[] EventModules { get; set; }
        //    public IDictionary<Guid, IDictionary<Guid, Parameter[]>> Parameters { get; set; } //eventModuleId => [updaterId => updaterParameters]
        //}

        ////public interface IEvent { }
        ////public class Event : IEvent  { }

        //public class EventModuleDefinitionModel { }
        //public class EventModuleDescriptionModel { }
        //public class EventModuleModel0
        //{
        //    public EventModuleDefinitionModel definition { get; }
        //    public EventModuleDescriptionModel Description { get; }
        //}

        ////public class EventModuleDescription
        ////{
        ////    public Guid Id { get; set; }
        ////    public string Name { get; set; }
        ////    public string Description { get; set; }

        ////    public IDictionary<Guid, Parameter[]> Parameters { get; set; }
        ////}








    }
}

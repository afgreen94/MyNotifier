using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.EventModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IDefinition = MyNotifier.Contracts.Base.IDefinition;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Contracts
{
    //CURRENT SCHEMA 

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

    //----------------------------------------------------

    //{note 1.0}
    //updater definitions are abstract because an updater definition only comes into being when a new updater is written. By design, definitions will be created and wrapped in with updaters as they are written
    //other definition objects can be dynamically built. this functionality must be implemented. for now they are all abstractions 
    //definition objects are abstract, meaning specific values must correspond to hardcoded type
    //will need customizeable derivative type, protected type and use fluid-building to issue instances as implementations of corresponding interface 

    //parameter-common updater definitions ? all udefns share same parameterValue for same parameter defn 


    //for now, one updater per interest, will expand 
    //public class Interest
    //{
    //    public Definition Definition { get; set; }
    //    public UpdaterArgs UpdaterArgs { get; set; }
    //}

    //public class Interest
    //{
    //    public Definition Definition { get; set; }
    //    public UpdaterArgs UpdaterArgs { get; set; }
    //    //public PublicationChannelArgs PublicationChannelArgs { get; set; }

    //    //public Definition[] TypeHierarchy { get; set; }
    //    //public NotifierArgs NotifierArgs { get; set; } //per interest notififers adds currently unnecessary layer of complexity, maybe later 
    //}

    //public class InterestClass
    //{
    //    public Interest[] Interests { get; set; }
    //}

    //public class NotifierArgs
    //{
    //    public object FactoryArg { get; set; }
    //}

    //public class InterestDefinition : Definition
    //{
    //    public EventModuleDefinition EventModuleDefinition { get; set; }
    //}

    //public class Interest0
    //{
    //    public InterestDefinition Definition { get; set; }

    //    public EventModule[] EventModules { get; set; }
    //}


    public interface IInterestDefinition { }
    public class InterestDefinition { }
    public class CustomInterestDefinition : IInterestDefinition { }

    public class InterestDefinitionModel { }


    public interface IInterest 
    {
        IDefinition Definition { get; }
        IEventModule[] EventModules { get; }
    }

    public class Interest : IInterest
    {
        public IDefinition Definition { get; set; }
        public IEventModule[] EventModules { get; set; }
    }

    public class InterestModel
    {
        public Definition Definition { get; set; }
        public EventModuleModel[] EventModuleModels { get; set; }
    }

    public class Event
    {
        public Definition Definition { get; set; }
        public Definition InterestDefinition { get; set; }
    }
}

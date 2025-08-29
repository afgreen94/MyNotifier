using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyNotifier.Contracts
{

    //more on this later 

    public interface IParameterDefinition
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        ParameterType Type { get; }
        bool IsRequired { get; } 

        //[ParameterValue|object] defaultValue { get; } ??
    }

    public class ParameterDefinition : Definition, IParameterDefinition
    {
        public ParameterType Type { get; set; }
        public bool IsRequired { get; set; } = false;
    }

    //public class DeserializeableParameterDefinitionModel
    //{
    //    public ParameterType Type { get; set; }
    //    public bool IsRequired { get; set; }
    //}

    public class ParameterValue //necessary ? 
    {
        public Guid ParameterDefinitionId { get; set; }
        public object Value { get; set; }
    }
    public class Parameter 
    {
        public IParameterDefinition Definition { get; set; }
        public ParameterValue Value { get; set; }
    }

    public enum ParameterType
    {
        Byte,
        Char,
        String,
        Bool,
        Int,
        Long,
        Double
    }

    //base -> required by all updaters 
    //common -> useable by all updaters 
    //special -> usually specific to an updater 

    //need a way of setting props that differ between contexts, ie IsRequired 
    //should issue new instance for each call
    public class ParameterDefinitions
    {
        public class Base
        {
            public static IParameterDefinition NotificationReturnProtocol() => new ParameterDefinition()
            {
                Id = new Guid("{}"),
                Name = "Notification Return Protocol",
                Description = "Describes 'return protocol' of updater, represented as a string", //need more descriptive name for this
                IsRequired = true,
                Type = ParameterType.String
            };

            //update at TIME |required xor with| update every TIME_INTERVAL //sigh...need required xor system now...
        }

        public class Common
        {
            public static IParameterDefinition Name(bool isRequired = false) => new ParameterDefinition()
            {
                Id = new Guid("{}"),
                Name = "Name",
                Description = "Name attribute, represented as a string",
                IsRequired = isRequired
            };
        }
    }

}

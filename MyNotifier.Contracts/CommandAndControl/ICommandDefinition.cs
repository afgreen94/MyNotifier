using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface ICommandDefinition : IDefinition
    {
        Type ServiceType { get; }
        Type FactoryType { get; }
        Type CommandType { get; }
        IParameterDefinition[] ParameterDefinitions { get; }
    }
}

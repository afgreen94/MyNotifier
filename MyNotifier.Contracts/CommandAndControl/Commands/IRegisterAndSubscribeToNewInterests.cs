using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface IRegisterAndSubscribeToNewInterestsDefinition : ICommandDefinition { }
    public interface IRegisterAndSubscribeToNewInterests : ICommand { }

    public interface IRegisterAndSubscribeToNewInterestsWrapper : ICommandWrapper<IRegisterAndSubscribeToNewInterests>
    {
        public INewInterestModel[] NewInterestModels { get; }
    }

    public interface INewInterestModel
    {
        IDefinition Definition { get; set; }
        Guid[] EventModuleDefinitionIds { get; set; }
        IDictionary<Guid, IEventModuleParameterValues[]> ParameterValues { get; set; }
    }
}

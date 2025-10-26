using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface IControllable
    {
        IDefinition Definition { get; }
        ValueTask<ICommandResult> OnCommandAsync(ICommand command);
    }

    //public interface IControllable
    //{
    //    ValueTask<ICommandResult> OnCommandAsync(ICommand command);
    //}


    public interface IControllable<TCommand> : IControllable
        where TCommand : ICommand   
    {
        //Guid CommandDefinitionId { get; }
        ValueTask<ICommandResult<TCommand>> OnCommandAsync(TCommand command); //ICallResult? 
    }
}

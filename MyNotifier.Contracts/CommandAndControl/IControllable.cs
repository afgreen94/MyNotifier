using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.CommandAndControl.Commands;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface IControllable
    {
        IDefinition Definition { get; }
    }


    public interface IControllable<TCommand> : IControllable
        where TCommand : ICommand   
    {
        ValueTask OnCommandAsync(TCommand command);
    }
}

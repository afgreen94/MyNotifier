using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Base;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface ICommandWrapper<TCommand, TCommandParameters> 
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters
    {
        TCommand InnerCommand { get; }
        TCommandParameters Parameters { get; }
    }

    public interface ICommandBuilder<TCommand, TCommandParameters>
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters
    {
        ICallResult<TCommand> BuildFrom(TCommandParameters parameters, bool suppressValidation = false); 
    }

    public interface ICommandWrapperBuilder<TCommand, TCommandParameters, TCommandWrapper>
        where TCommand : ICommand
        where TCommandParameters : ICommandParameters
        where TCommandWrapper : ICommandWrapper<TCommand, TCommandParameters>
    {
        ICallResult<TCommandWrapper> BuildFrom(ICommand command, bool suppressValidation = false);
    }

}

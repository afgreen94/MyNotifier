using MyNotifier.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    public class CommandResult : CallResult, ICommandResult
    {
        public CommandResult() : base() { }
        public CommandResult(bool success, string errorText) : base(success, errorText) { }
    }

    public class CommandResult<TCommand> : CommandResult, ICommandResult<TCommand> where TCommand : ICommand
    {
        public CommandResult() : base() { }
        public CommandResult(bool success, string errorText) : base(success, errorText) { }
    }
}

using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface ICommandApi
    {
        Task<ICallResult> IssueCommandAsync(ICommand command);
        Task<ICallResult> IssueCommandAwaitResultAsync(ICommand command);
    }
}

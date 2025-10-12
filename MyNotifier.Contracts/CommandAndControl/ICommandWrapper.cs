using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl
{
    public interface ICommandWrapper<T> where T : ICommand
    {
        T InnerCommand { get; }
    }

    public interface ICommandWrapperBuilder
    {
        ICommandWrapper<T> BuildFrom<T>(T command) where T : ICommand;
    }
}

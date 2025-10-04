using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface ISubscribeToInterestsByIdDefinition : ICommandDefinition { }
    public interface ISubscribeToNewInterestsById : ICommand { }
}

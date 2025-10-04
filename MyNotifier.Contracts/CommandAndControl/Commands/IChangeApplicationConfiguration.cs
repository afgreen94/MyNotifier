using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.CommandAndControl.Commands
{
    public interface IChangeApplicationConfigurationDefinition : ICommandDefinition { }
    public interface IChangeApplicationConfiguration : ICommand { }
}

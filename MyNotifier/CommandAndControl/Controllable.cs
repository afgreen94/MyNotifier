using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.CommandAndControl
{
    public abstract class Controllable<TCommand> : IControllable
                where TCommand : ICommand
    {
        public abstract IDefinition Definition { get; }

        public virtual async ValueTask OnCommandAsync(ICommand command)
        {
            //validate 

            if (command is not TCommand coreCommand) throw new Exception("I always forget if C# allows narrowing casts. probably not explicityly tbh.");

            await this.OnCommandCoreAsync(coreCommand).ConfigureAwait(false); //does C# allow narrowing casts? I always forget this...
        }

        protected abstract ValueTask OnCommandCoreAsync(TCommand command);

        ValueTask<ICommandResult> IControllable.OnCommandAsync(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}

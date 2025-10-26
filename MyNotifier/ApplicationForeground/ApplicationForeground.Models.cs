using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {

        public class UpdateAvailableArgs
        {
            public IInterest Interest { get; set; }
            public IEventModule EventModule { get; set; }
            public IUpdater Updater { get; set; }
            public IUpdaterResult Result { get; set; }
        }

        public class CommandArgs
        {
            public ICommand Command { get; set; }
        }
        public class FailureArgs
        {
            public ICallResult FailedResult { get; set; }
        }
        public class HandleFailureArgs { }


        public interface IUpdateSubscriber
        {
            Guid Id { get; }
            void OnUpdateAvailable(UpdateAvailableArgs args);
        }

        public interface ICommandNotifierSubscriber
        {
            ValueTask<ICommandResult> OnCommandAvailableAsync(ICommand command);
        }

        public interface IFailureSubscriber
        {
            Guid Id { get; }
            void OnFailure(FailureArgs args);
        }

        public interface ICommandSubscriber
        {
            Guid Id { get; }
            void OnCommand(CommandArgs args);
        }


        public interface IBackgroundable
        {
            ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failedResult);
        }
    }
}

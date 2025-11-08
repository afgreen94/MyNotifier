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
        public class TaskCompleteArgs
        {
            public BackgroundTaskData TaskData { get; set; }
            public ICallResult Result { get; set; }

            //include return value ?
        }

        public class FailureArgs
        {
            public ICallResult FailedResult { get; set; } 
            //Exception ?
        }
        public class HandleFailureArgs { }


        public interface ISubscriber
        {
            Guid Id { get; }
        }

        public interface IUpdateSubscriber : ISubscriber
        {
            void OnUpdateAvailable(UpdateAvailableArgs args);
        }

        public interface ICommandNotifierSubscriber : ISubscriber
        {
            ValueTask<ICommandResult> OnCommandAvailableAsync(ICommand command);
        }

        public interface ITaskCompleteSubscriber : ISubscriber
        {
            void OnTaskComplete(TaskCompleteArgs args);
        }

        public interface IFailureSubscriber : ISubscriber
        {
            void OnFailure(FailureArgs args);
        }

        public interface ICommandSubscriber : ISubscriber
        {
            void OnCommand(CommandArgs args);
        }


        public interface IBackgroundable
        {
            ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failedResult);
        }
    }
}

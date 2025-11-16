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
        public class CommandArgs
        {
            public ICommand Command { get; set; }
        }
        public class TaskCompleteArgs
        { 
            //include reference to backgroundTaskWrapper ?
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

        public interface ICommandNotifierSubscriber : MyNotifier.Contracts.ISubscriber
        {
            ValueTask<ICommandResult> OnCommandAvailableAsync(ICommand command);
        }

        public interface ICommandSubscriber : MyNotifier.Contracts.ISubscriber
        {
            void OnCommand(CommandArgs args);
        }

        //put in backgrounding ? 
        public interface ITaskCompleteSubscriber : MyNotifier.Contracts.ISubscriber
        {
            void OnTaskComplete(TaskCompleteArgs args);
        }

        public interface IFailureSubscriber : MyNotifier.Contracts.ISubscriber
        {
            void OnFailure(FailureArgs args);
        }

        public interface IBackgroundable
        {
            ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failedResult);
        }
    }
}

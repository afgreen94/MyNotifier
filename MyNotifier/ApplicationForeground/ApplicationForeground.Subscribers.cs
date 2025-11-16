using MyNotifier.CommandAndControl;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.ApplicationForeground;
using IUpdateSubscriber = MyNotifier.Contracts.Updaters.ISubscriber;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {
        public class Subscribers
        {
            public abstract class Subscriber 
            {
                protected readonly MessageQueue messageQueue;

                protected Subscriber(MessageQueue messageQueue) { this.messageQueue = messageQueue; }
            }

            public class UpdateAvailableSubscriber : Subscriber, IUpdateSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public UpdateAvailableSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnUpdateAvailable(UpdateAvailableArgs args) => this.messageQueue.Enqueue(new UpdateAvailableMessage(args));
            }

            public class CommandSubscriber : Subscriber, ICommandSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public CommandSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnCommand(CommandArgs args)
                {
                    throw new NotImplementedException();
                }
            }

            //may not need subscribers for application-generic events like taskcomplete & onfailure. could be more appropriate to ref MessageQueue directly in Backgrounding Manager. ultimately, doesn't make much practical difference, for now
            public class TaskCompleteSubscriber : Subscriber, ITaskCompleteSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public TaskCompleteSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnTaskComplete(TaskCompleteArgs args) { }
            }

            public class FailureSubscriber : Subscriber, IFailureSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public FailureSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnFailure(FailureArgs args) { } //require handleFailure return value to background caller ? //have background caller lock failure message? give backgrounder direct ref to message queue ? //all failure logic in foreground?
            }
        }

        //public class Handler(MessageQueue messageQueue) : INotifier.ISubscriber //CommandNotifierSubscriber 
        //{
        //    private readonly MessageQueue messageQueue = messageQueue;

        //    public Definition Definition => throw new NotImplementedException();

        //    public async ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failedResult, bool expectingResult = false)
        //    {
        //        var handleFailureArgs = new HandleFailureArgs();

        //        var failureMessage = new FailureMessage(new FailureArgs() { FailedResult = failedResult }, expectingResult);
        //        this.messageQueue.Enqueue(failureMessage);

        //        if (!expectingResult) //this should be handled by caller 
        //        {
        //            //get handle failure result 
        //        }

        //        return handleFailureArgs;
        //    }

        //    public async ValueTask OnNotificationAsync(object sender, Notification notification)
        //    {
        //        ICommand command = default; //build from notification

        //        var result = await this.OnCommandAsync(command).ConfigureAwait(false);
        //    }

        //    private async ValueTask<ICommandResult> OnCommandAsync(ICommand command, bool continueWithoutResult = false) //drop in message queue, separate service for OnCommandAvailable?
        //    {
        //        var message = new CommandIssuedMessage(command);
        //        this.messageQueue.Enqueue(message);

        //        ICommandResult result = new CommandResult();
        //        bool expectingCommandResult = true;

        //        if (expectingCommandResult) //should release event thread, relocate command result some other way 
        //        {
        //            //awaiting here is probably redundant 
        //            await Task.Run(() => { while (message.Status != MessageStatus.Processed && message.Status != MessageStatus.Faulted) { } }).ConfigureAwait(false); //need cancel flag ?

        //            if (message.Status == MessageStatus.Faulted) { /* handle */}

        //            //could lock message. probably doesn't matter 
        //            result = message.Result;
        //            message.Status = MessageStatus.Processed;
        //        }

        //        return result;
        //    }
        }
    }
}

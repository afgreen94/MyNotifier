using MyNotifier.CommandAndControl;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.ApplicationForeground;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {
        public class Subscribers
        {
            public abstract class SubscriberBase
            {
                protected readonly MessageQueue messageQueue;

                protected SubscriberBase(MessageQueue messageQueue) { this.messageQueue = messageQueue; }
            }

            public class UpdateAvailableSubscriber : SubscriberBase, IUpdateSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public UpdateAvailableSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnUpdateAvailable(UpdateAvailableArgs args) => this.messageQueue.Enqueue(new UpdateAvailableMessage(args));
            }

            public class CommandSubscriber : SubscriberBase, ICommandSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public CommandSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnCommand(CommandArgs args)
                {
                    throw new NotImplementedException();
                }
            }

            public class FailureSubscriber : SubscriberBase, IFailureSubscriber
            {
                private readonly Guid id = new("");
                public Guid Id => this.id;

                public FailureSubscriber(MessageQueue messageQueue) : base(messageQueue) { }

                public void OnFailure(FailureArgs args) { }
            }
        }

        public class Handler(MessageQueue messageQueue) : INotifier.ISubscriber //CommandNotifierSubscriber 
        {
            private readonly MessageQueue messageQueue = messageQueue;

            public Definition Definition => throw new NotImplementedException();

            public async ValueTask<HandleFailureArgs> OnFailureAsync(ICallResult failedResult, bool expectingResult = false)
            {
                var handleFailureArgs = new HandleFailureArgs();

                var failureMessage = new FailureMessage(new FailureArgs() { FailedResult = failedResult }, expectingResult);
                this.messageQueue.Enqueue(failureMessage);

                if (!expectingResult) //this should be handled by caller 
                {
                    //get handle failure result 
                }

                return handleFailureArgs;
            }

            public async ValueTask OnNotificationAsync(object sender, Notification notification)
            {
                ICommand command = default; //build from notification

                var result = await this.OnCommandAsync(command).ConfigureAwait(false);
            }

            private async ValueTask<ICommandResult> OnCommandAsync(ICommand command, bool continueWithoutResult = false) //drop in message queue, separate service for OnCommandAvailable?
            {
                var message = new CommandIssuedMessage(command);
                this.messageQueue.Enqueue(message);

                ICommandResult result = new CommandResult();
                bool expectingCommandResult = true;

                if (expectingCommandResult) //should release event thread, relocate command result some other way 
                {
                    //awaiting here is probably redundant 
                    await Task.Run(() => { while (message.Status != MessageStatus.Processed && message.Status != MessageStatus.Faulted) { } }).ConfigureAwait(false); //need cancel flag ?

                    if (message.Status == MessageStatus.Faulted) { /* handle */}

                    //could lock message. probably doesn't matter 
                    result = message.Result;
                    message.Status = MessageStatus.Processed;
                }

                return result;
            }
        }
    }
}

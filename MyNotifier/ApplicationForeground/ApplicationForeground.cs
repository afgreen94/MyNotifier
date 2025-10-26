using MyNotifier.Base;
using MyNotifier.CommandAndControl;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyNotifier.ApplicationForeground.Subscribers;
using static MyNotifier.FileIOManager.FileIOManager;
using IInterestManager = MyNotifier.Contracts.Interests.IManager;

namespace MyNotifier
{
    public partial class ApplicationForeground
    {
        private readonly IInterestManager interestManager;
        private readonly ICommandAndController controller;
        private readonly INotifierPublisher publisher;
        private readonly IApplicationConfiguration configuration;
        private readonly ICallContext<ApplicationForeground> callContext;

        private readonly NotificationBuilder notificationBuilder = new();

        private readonly MessageQueue messageQueue = new();
         
        private readonly Handler handler;

        private readonly UpdateAvailableSubscriber updateAvailableHandler;
        private readonly CommandSubscriber commandSubscriberHandler;
        private readonly FailureSubscriber failureSubscriberHandler;


        public ApplicationForeground(IServiceProvider serviceProvider, IApplicationConfiguration configuration) { } //just give it a service provider lol ... appropriate at this level !

        public ApplicationForeground(IInterestManager interestManager,
                                     ICommandAndController controller,
                                     INotifierPublisher publisher,
                                     IApplicationConfiguration configuration, 
                                     ICallContext<ApplicationForeground> callContext)
        {
            this.interestManager = interestManager;
            this.controller = controller;
            this.publisher = publisher;
            this.configuration = configuration;
            this.callContext = callContext;
            
            this.handler = new(this.messageQueue);

            this.updateAvailableHandler = new(this.messageQueue);
            this.commandSubscriberHandler = new(this.messageQueue);
            this.failureSubscriberHandler = new(this.messageQueue);
        }

        public async Task RunAsync()
        {
            await this.InitializeAsync().ConfigureAwait(false);

            var cancelFlag = new BooleanFlag();

            while (!cancelFlag.Value)
            {
                while (this.messageQueue.TryDequeue(out var message))
                {
                    if (message.Status == MessageStatus.Processed) { continue; } //remove //continue
                    if (message.Status == MessageStatus.Faulted) { } //?

                    message.Status = MessageStatus.Processing;

                    ICallResult processResult = message.Type switch
                    {
                        MessageType.Update => await this.ProcessUpdateMessageAsync(message).ConfigureAwait(false),
                        MessageType.Command => await this.ProcessCommandIssuedMessageAsync(message).ConfigureAwait(false),
                        MessageType.Failure => await this.ProcessFailureMessageAsync(message).ConfigureAwait(false),
                        _ => throw new NotImplementedException() //never reached 
                    };

                    if (!processResult.Success) { message.Status = MessageStatus.Faulted; /*log, maybe inform client, depending on failure*/ } //cache messages ?
                }
            }
        }

        private async Task InitializeAsync() { }

        public static ApplicationForeground Build(IServiceProvider serviceProvider, IApplicationConfiguration applicationConfiguration) => throw new NotImplementedException();
    }
}

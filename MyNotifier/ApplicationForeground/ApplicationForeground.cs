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
using System.Security;
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
        private readonly ICommandNotifierWrapper commandNotifier;
        private readonly INotifierPublisher publisher;
        private readonly IBackgroundTaskManager backgrounder;
        private readonly IApplicationConfiguration configuration;
        private readonly ICallContext<ApplicationForeground> callContext;

        private readonly NotificationBuilder notificationBuilder = new();

        private readonly MessageQueue messageQueue = new();

        private readonly UpdateAvailableSubscriber updateAvailableSubscriber;
        private readonly CommandSubscriber commandSubscriber;
        private readonly TaskCompleteSubscriber taskCompleteSubscriber;
        private readonly FailureSubscriber failureSubscriber;


        public ApplicationForeground(IServiceProvider serviceProvider, IApplicationConfiguration configuration) { } //just give it a service provider lol ... appropriate at this level !

        public ApplicationForeground(IInterestManager interestManager,
                                     ICommandAndController controller,
                                     INotifierPublisher publisher,
                                     ICommandNotifierWrapper commandNotifier,
                                     IBackgroundTaskManager backgrounder,
                                     IApplicationConfiguration configuration,
                                     ICallContext<ApplicationForeground> callContext)
        {
            this.interestManager = interestManager;
            this.controller = controller;
            this.commandNotifier = commandNotifier;
            this.publisher = publisher;
            this.backgrounder = backgrounder;
            this.configuration = configuration;
            this.callContext = callContext;

            this.updateAvailableSubscriber = new(this.messageQueue);
            this.commandSubscriber = new(this.messageQueue);
            this.taskCompleteSubscriber = new(this.messageQueue);
            this.failureSubscriber = new(this.messageQueue);
        }

        public async Task RunAsync(IInterest[] sessionInterests, SecureString sessionKey) 
        {
            await this.InitializeAsync(sessionInterests, sessionKey).ConfigureAwait(false);

            try
            {
                var cancelFlag = new BooleanFlag();

                while (!cancelFlag.Value)
                {
                    while (this.messageQueue.TryDequeue(out var message))
                    {
                        if (message.Status == MessageStatus.Processed) { continue; } //remove //continue  ???
                        if (message.Status == MessageStatus.Faulted) { } //?

                        message.Status = MessageStatus.Processing;

                        ICallResult processResult = message.Type switch
                        {
                            MessageType.Update => await this.ProcessUpdateMessageAsync(message).ConfigureAwait(false),
                            MessageType.Command => await this.ProcessCommandIssuedMessageAsync(message).ConfigureAwait(false),
                            MessageType.TaskComplete => await this.ProcessTaskCompleteMessageAsync(message).ConfigureAwait(false),
                            MessageType.Failure => await this.ProcessFailureMessageAsync(message).ConfigureAwait(false),
                            _ => throw new NotImplementedException() //never reached 
                        };

                        if (!processResult.Success) { message.Status = MessageStatus.Faulted; }  /*fatal error? log, maybe inform client, depending on failure*/  //cache messages ?
                    }
                }
            }
            catch (Exception ex) { } //fatal error
        }

        //abstract initializations, eg IInitializable + Initializer class with InitializeAll(IInitializable[]) 
        private async Task InitializeAsync(IInterest[] sessionInterests, SecureString sessionKey)
        {

            //initialize background task manager //abstract this !!! 
            var initializeBackgrounderResult = await this.backgrounder.InitializeAsync(this.taskCompleteSubscriber, this.failureSubscriber).ConfigureAwait(false);
            if(!initializeBackgrounderResult.Success) throw new Exception($"Failed to initialize background task manager: {initializeBackgrounderResult.ErrorText}"); //fatal error  //log !!!

            //initialize components 
            var initializeComponentsResult = await Initializer.InitializeAllAsync([this.publisher, this.controller]).ConfigureAwait(false); //should initializer get type friendly-names ?
            if(!initializeComponentsResult.Success) throw new Exception($"Failed to initialize component: {initializeComponentsResult.ErrorText}"); //fatal error  //log !!!

            //register controllables 
            this.controller.Registrar.Register(this.interestManager.Controllable, this.configuration.Controllable);

            //initialize notifier 
            //register as notifier subscriber 
            this.commandNotifier.RegisterCommandSubscriber(this.commandSubscriber);

            //initialize interest manager
            //register as interest manager subscriber
            this.interestManager.RegisterUpdateSubscriber(this.updateAvailableSubscriber);

            //connect notifier 
            var connectResult = await this.commandNotifier.ConnectAsync(null).ConfigureAwait(false);
            if(!connectResult.Success) throw new Exception($"Failed to connect command notifier: {connectResult.ErrorText}"); //fatal error  //log !!!


            //add start live interests
            foreach(var interest in sessionInterests)
            {
                var addInterestResult = this.interestManager.AddStartInterest(interest);
                if(!addInterestResult.Success) throw new Exception($"Failed to add interest {interest.Definition.Name}: {addInterestResult.ErrorText}"); //fatal error  //log !!! //publish error?
            }

            sessionKey.Dispose(); //sensitive data
        }

        //public static ApplicationForeground Build(IServiceProvider serviceProvider, IApplicationConfiguration applicationConfiguration) => throw new NotImplementedException();
    }
}

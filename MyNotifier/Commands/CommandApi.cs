using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Commands
{



    public class CommandApi : ICommandApi  //could wrap entire command flow, including awaiting command result notification 
    {

        private readonly INotifierPublisherFactory publisherFactory;
        private readonly IConfiguration configuration;
        private readonly ICallContext<CommandApi> callContext;

        private readonly HashSet<Type> supportedCommandDefinitionTypes =  //from config 
        [
            typeof(IChangeApplicationConfigurationDefinition),
            typeof(IRegisterAndSubscribeToNewInterestsDefinition),
            typeof(ISubscribeToInterestsByDefinitionIds),
            typeof(IUnsubscribeFromInterestsByIdDefinition)
        ];

        private readonly CommandValidator commandValidator;

        private readonly Encoding defaultEncoding = Encoding.UTF8; //should come from config
        private readonly string defaultCommandDescription = "JSON.UTF8"; //should be built based on encoding from config 

        private INotifierPublisher publisher;

        private bool isInitialized = false;

        public CommandApi(INotifierPublisherFactory publisherFactory,
                          IConfiguration configuration,
                          ICallContext<CommandApi> callContext) 
        {
            this.publisherFactory = publisherFactory;
            this.configuration = configuration; 
            this.callContext = callContext;

            this.commandValidator = new(configuration);
        }


        //public void UsePublisherAndNotifier(INotifierPublisher publisher, INotifier notifier) { this.publisher = publisher; this.notifier = notifier; }  //rather than injection via ctor 

        public async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false)
        {
            try
            {
                if (!this.isInitialized || forceReinitialize)
                {
                    this.publisher = this.publisherFactory.GetNotifierPublisher();

                    var initializePublisherResult = await this.publisher.InitializeAsync().ConfigureAwait(false);
                    if (!initializePublisherResult.Success) return CallResult.BuildFailedCallResult(initializePublisherResult, "Failed to initialize command publisher: {0}");

                    this.isInitialized = true;
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public async Task<ICallResult> IssueCommandAsync(ICommand command)
        {
            try
            {
                if (!this.isInitialized) return new CallResult(false, "Not Initialized.");

                //var commandType = command.GetType();
                if (!this.supportedCommandDefinitionTypes.Contains(command.Definition.ServiceType)) return new CallResult(false, $"Unsupported command type: {command.Definition.Name}");
                if (!this.commandValidator.TryValidateCommand(command, out var errorText)) return new CallResult(false, $"Invalid command: {errorText}");

                var commandJson = JsonSerializer.Serialize(command);
                var data = this.defaultEncoding.GetBytes(commandJson);

                var publishResult = await this.publisher.PublishAsync(new PublishArgs()
                {
                    InterestId = Guid.Empty,
                    UpdaterId = Guid.Empty,
                    TypeArgs = new TypeArgs()
                    {
                        NotificationType = NotificationType.Command,
                        NotificationDataTypeArgs = new DataTypeArgs()
                        {
                            DataType = NotificationDataType.String_Json,
                            Description = this.defaultCommandDescription  //ultimately, encoding should be configurable 
                        }
                    },
                    UpdateTime = DateTime.UtcNow,
                    Data = data
                }).ConfigureAwait(false);

                if (!publishResult.Success) return CallResult.BuildFailedCallResult(publishResult, "Publish command failed: {0}");

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public Task<ICallResult> IssueCommandAwaitResultAsync(ICommand command) => throw new NotImplementedException(); //will require notifier, notifier will have to suppress unrelated commands/require special config to ignore non-command results 


        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration) { }
        }

        public class CommandValidator
        {
            private readonly IConfiguration configuration;

            public CommandValidator(IConfiguration configuration) { this.configuration = configuration; }

            public bool TryValidateCommand(ICommand command, out string errorText) => throw new NotImplementedException();
        }
    }
}

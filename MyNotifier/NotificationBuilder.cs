using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyNotifier
{
    public class NotificationBuilder
    {

        private readonly Encoding defaultEncoding = Encoding.UTF8; //make configurable, although serializer I think generally assumes UTF8 

        public Notification Build(IInterest interest,  //can provide custom id ?
                                  IEventModule eventModule,
                                  IUpdater updater,
                                  IUpdaterResult result) =>
            new() //excludes all publish time params, should be handled by publisher 
            {
                Metadata = new()
                {
                    Description = new UpdateNotificationDescription()
                    {
                        Header = new NotificationHeader() { Id = Guid.NewGuid() },
                        //InterestDefinitionId = args.Interest.Definition.Id, //haven't implemented interest definitions yet !!! 
                        InterestId = interest.Definition.Id,
                        EventModuleDefinitionId = eventModule.Definition.Id,
                        //EventModuleId = args.EventModule.Id, //haven't implemented eventModule id yet !!! 
                        UpdaterDefinitionId = updater.Definition.Id,
                        //UpdaterId = args.Updater.Id //haven't implemented update Id yet !!! 

                        UpdatedAt = result.UpdatedAt,
                        //PublishedTo = new() //havent implemented publishedTo yet //maybe handled by publisher 
                    },
                    DataTypeArgs = result.TypeArgs,
                    SizeBytes = result.Data.Length,
                    Encrypted = false //haven't implemented encryption yet !!! 
                },
                Data = result.Data,
            };

        public Notification Build(IEventModule eventModule, IUpdater updater, IUpdaterResult result) => throw new NotImplementedException();
        public Notification Build(IUpdater updater, IUpdaterResult result) => throw new NotImplementedException();
        public Notification Build(IUpdaterResult result) => throw new NotImplementedException();
        public Notification Build(ICommand command)
        {
            var data = this.defaultEncoding.GetBytes(JsonSerializer.Serialize(command));

            return new Notification()
            {
                Metadata = new()
                {
                    Description = new()
                    {
                        Header = new() { Id = Guid.NewGuid(), Type = NotificationType.Command },
                        UpdatedAt = DateTime.UtcNow //in case of command, update is time of command issuance, could abstract property name to be more clear 
                    },
                    DataTypeArgs = new() { DataType = NotificationDataType.String_Json, Description = $"{this.defaultEncoding.EncodingName} json representation of command of type {command.Definition.Name} with Id: {command.Definition.Id}" }, //standardize this message !
                    Encrypted = false,
                    SizeBytes = data.Length
                },
                Data = data
            };
        }
        public Notification Build(ICommandResult commandResult) => throw new NotImplementedException();
        public Notification Build(ICallResult failedResult) => throw new NotImplementedException();

    }
}

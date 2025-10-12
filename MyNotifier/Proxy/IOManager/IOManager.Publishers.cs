using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Proxy
{
    public abstract partial class IOManager : IIOManager 
    {

        private const string UnknownWriteTaskErrorMessage = "Write notification task failed, error unknown.";
        private const string FailedToCreateNotificationDirectoryMessage = "Failed to create notification directory: {0}";

        protected readonly INotificationFileSystemObjectTranslator notificationFileSystemObjectTranslator;

        public virtual async ValueTask<ICallResult> WriteNotificationFilesAsync(Notification notification)  //WriteNotificationAsync ? 
        {
            try
            {
                if (!this.isInitialized) return new CallResult(false, NotInitializedMessage);

                var notificationDirectoryPath = this.BuildNotificationDirectoryPath(notification);

                var createDirectoryResult = await this.fileIOManager.CreateDirectoryAsync(notificationDirectoryPath).ConfigureAwait(false);
                if (!createDirectoryResult.Success) return CallResult.BuildFailedCallResult(createDirectoryResult, FailedToCreateNotificationDirectoryMessage);


                

                var writeNotificationMetadataFileTask = this.WriteNotificationFilesAsync(this.BuildNotificationMetadataFilePath(notificationDirectoryPath), this.GetMetadataBytes(notification.Metadata));
                var writeNotificationFileTask = this.WriteNotificationFilesAsync(this.BuildNotificationDataFilePath(notificationDirectoryPath), notification.Data);

                await Task.WhenAll(writeNotificationMetadataFileTask, writeNotificationFileTask).ConfigureAwait(false);


                if (!writeNotificationFileTask.IsCompletedSuccessfully) return this.BuildCallResultForFailedWriteTask(writeNotificationFileTask);
                if (!writeNotificationMetadataFileTask.IsCompletedSuccessfully) return this.BuildCallResultForFailedWriteTask(writeNotificationMetadataFileTask);

                return new CallResult();

            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected async Task WriteNotificationFilesAsync(string filePath, byte[] data) //this needs to be reworked. notification filepath format should be pulled from configuration //encapsulate filePath formatting !!! 
        {
            //using var sw = new StreamWriter(createWriteStreamResult.Result);

            if (data != null && data.Length != 0)
            {
                var createWriteStreamResult = this.fileIOManager.CreateWriteFileStream(filePath, FileMode.CreateNew, this.configuration.AllowOverwriteExistingNotification);
                if (!createWriteStreamResult.Success) throw new Exception(createWriteStreamResult.ErrorText); // fix this 

                using var writeStream = createWriteStreamResult.Result;
                await writeStream.WriteAsync(data).ConfigureAwait(false);
            }

            //!!! 
            //add wait() to allow for write to complete?
            //use uploadAsync() instead of createWriteStream.Write() ?? 
            //add verification of write ?
            //check files exist on FileServer with expected size ?
        }

        protected byte[] GetMetadataBytes(NotificationMetadata metadata) => this.defaultEncoding.GetBytes(JsonSerializer.Serialize(this.GetMetadataModel(metadata)));

        protected NotificationMetadataModel GetMetadataModel(NotificationMetadata metadata)
        {
            var model = new NotificationMetadataModel
            {
                DataTypeArgs = new DataTypeArgsModel() { DataType = Contracts.EnumStringMaps.GetString(metadata.DataTypeArgs.DataType), Description = metadata.DataTypeArgs.Description },
                Encrypted = metadata.Encrypted,
                SizeBytes = metadata.SizeBytes,
                Description = new NotificationDescriptionModel()
                {
                    Id = metadata.Description.Header.Id,
                    PublishedAt = metadata.Description.PublishedAt,
                    UpdatedAt = metadata.Description.UpdatedAt,
                    PublishedTo = metadata.Description.PublishedTo
                }
            };

            if (metadata.Description is UpdateNotificationDescription updateNotificationDescription)
            {
                model.Description.InterestDefinitionId = updateNotificationDescription.InterestDefinitionId;
                model.Description.InterestId = updateNotificationDescription.InterestId;
                model.Description.EventModuleDefinitionId = updateNotificationDescription.EventModuleDefinitionId;
                model.Description.EventModuleId = updateNotificationDescription.EventModuleId;
                model.Description.UpdaterDefinitionId = updateNotificationDescription.UpdaterDefinitionId;
                model.Description.UpdaterId = updateNotificationDescription.UpdaterId;
            }
            else if (metadata.Description is CommandResultNotificationDescription commandResultNotificationDescription) { model.Description.CommandNotificationId = commandResultNotificationDescription.CommandNotificationId; }

            return model;
        }


        //ultimately, scheme should be configurable (use fileIOManager & config)
        protected string BuildNotificationDirectoryPath(Notification notification) => this.fileIOManagerWrapper.BuildAppendedPath(this.configuration.PublishDirectoryRoot, this.ToFolderName(notification.Metadata.Description.Header)); 

        protected string BuildNotificationMetadataFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.DefaultMetadataFileName);
        protected string BuildNotificationDataFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.DefaultDataFileName);
        //protected string BuildCreateWriteCompleteSignalFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.WriteCompleteSignalArgs.Name);
        protected string BuildFilePath(string notificationDirectoryPath, string fileName) => this.fileIOManagerWrapper.BuildAppendedPath(notificationDirectoryPath, fileName);

        protected ICallResult BuildCallResultForFailedWriteTask(Task writeTask) => new CallResult(false, writeTask.Exception != null ? writeTask.Exception.Message : UnknownWriteTaskErrorMessage);
    }
}

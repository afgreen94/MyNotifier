using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;

namespace MyNotifier.Publishers
{
    public partial class FileNotifierPublisher
    {
        protected class PublisherFileIOHelper //: IProxyFileServerNotifierPublisherInterfacing
        {
            private const string UnknownWriteTaskErrorMessage = "Write notification task failed, error unknown.";
            private const string FailedToCreateNotificationDirectoryMessage = "Failed to create notification directory: {0}";

            private readonly IFileIOManager fileIOManager;
            private readonly IConfiguration configuration;

            private Encoding defaultEncoding = Encoding.UTF8; //encoding should be configurable, defaults to utf8 

            private PathBuilder pathBuilder;

            public PublisherFileIOHelper(IFileIOManager fileIOManager, INotificationFileSystemObjectTranslator translator, IConfiguration configuration)
            {
                this.fileIOManager = fileIOManager;
                this.configuration = configuration;

                this.pathBuilder = new PathBuilder(new FileIOManager.FileIOManager.Wrapper(this.fileIOManager), translator, configuration);
            }

            public async ValueTask<ICallResult> WriteNotificationFilesAsync(Notification notification)
            {
                try
                {
                    var notificationDirectoryPath = this.pathBuilder.BuildNotificationDirectoryPath(notification);

                    var createDirectoryResult = await this.fileIOManager.CreateDirectoryAsync(notificationDirectoryPath).ConfigureAwait(false);
                    if (!createDirectoryResult.Success) return CallResult.BuildFailedCallResult(createDirectoryResult, FailedToCreateNotificationDirectoryMessage);

                    var writeNotificationMetadataFileTask = this.WriteNotificationFilesAsync(this.pathBuilder.BuildNotificationMetadataFilePath(notificationDirectoryPath), this.GetMetadataBytes(notification.Metadata));
                    var writeNotificationFileTask = this.WriteNotificationFilesAsync(this.pathBuilder.BuildNotificationDataFilePath(notificationDirectoryPath), notification.Data);

                    await Task.WhenAll(writeNotificationMetadataFileTask, writeNotificationFileTask).ConfigureAwait(false);

                    //write WRITE_COMPLETE signal file
                    if(this.configuration.WriteCompleteSignalArgs != null)
                    {
                        var writeSignalFileResult = await this.CreateWriteCompleteSignalFileAsync(notificationDirectoryPath).ConfigureAwait(false);
                        if (!writeSignalFileResult.Success) return writeSignalFileResult;
                    }

                    if (!writeNotificationFileTask.IsCompletedSuccessfully) return this.BuildCallResultForFailedWriteTask(writeNotificationFileTask);
                    if (!writeNotificationMetadataFileTask.IsCompletedSuccessfully) return this.BuildCallResultForFailedWriteTask(writeNotificationMetadataFileTask);

                    return new CallResult();
                }
                catch (Exception ex) { return CallResult.FromException(ex); }
            }

            private async Task WriteNotificationFilesAsync(string filePath, byte[] data)
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

            private async Task<ICallResult> CreateWriteCompleteSignalFileAsync(string notificationDirectoryPath)
            {
                try
                {
                    var path = this.pathBuilder.BuildCreateWriteCompleteSignalFilePath(notificationDirectoryPath);

                    var createFileResult = await this.fileIOManager.CreateFileAsync(path, true).ConfigureAwait(false);
                    if (!createFileResult.Success) return CallResult.BuildFailedCallResult(createFileResult, "Failed to create signal file: {0}");

                    return new CallResult();
                }
                catch (Exception ex) { return CallResult.FromException(ex); }

            }

            private byte[] GetMetadataBytes(NotificationMetadata metadata) => this.defaultEncoding.GetBytes(JsonSerializer.Serialize(metadata));


            //private ICallResult<PublishResult> BuildCallResultForFailedWriteTask(Task writeTask) => new CallResult<PublishResult>(false, writeTask.Exception != null ? writeTask.Exception.Message : UnknownWriteTaskErrorMessage);
            private ICallResult BuildCallResultForFailedWriteTask(Task writeTask) => new CallResult(false, writeTask.Exception != null ? writeTask.Exception.Message : UnknownWriteTaskErrorMessage);


            protected class PathBuilder  //this needs to be reworked. notification filepath format should be pulled from configuration //encapsulate filePath formatting !!! 
            {
                //use to build paths
                private readonly IFileIOManager.IWrapper fileIOManager;
                private readonly INotificationFileSystemObjectTranslator translator;
                private readonly IConfiguration configuration;

                public PathBuilder(IFileIOManager.IWrapper fileIOManager, 
                                   INotificationFileSystemObjectTranslator translator, 
                                   IConfiguration configuration) 
                { 
                    this.fileIOManager = fileIOManager;
                    this.translator = translator;
                    this.configuration = configuration; 
                }

                public string BuildNotificationDirectoryPath(Notification notification) //ultimately, scheme should be configurable (use fileIOManager & config)
                {
                    var folderName = this.translator.ToFolderName(new NotificationFolderObjectDescription()
                    {
                        Ticks = notification.Metadata.UpdatedAt.Ticks,
                        Type = notification.Metadata.TypeArgs.NotificationType,
                        InterestId = notification.Metadata.Definition.InterestId,
                        UpdaterId = notification.Metadata.Definition.UpdaterId,
                    });

                    return this.fileIOManager.BuildAppendedPath(this.configuration.PublishDirectoryRoot, folderName);
                }

                public string BuildNotificationMetadataFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.DefaultMetadataFileName);
                public string BuildNotificationDataFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.DefaultDataFileName);
                public string BuildCreateWriteCompleteSignalFilePath(string notificationDirectoryPath) => BuildFilePath(notificationDirectoryPath, this.configuration.WriteCompleteSignalArgs.Name);
                private string BuildFilePath(string notificationDirectoryPath, string fileName) => this.fileIOManager.BuildAppendedPath(notificationDirectoryPath, fileName);
            }

        }
        protected class NotificationBuilder
        {

            private readonly IConfiguration configuration;

            public NotificationBuilder(IConfiguration configuration) { this.configuration = configuration; }

            public Notification BuildNotification(PublishArgs args) => new()
            {
                Metadata = new NotificationMetadata()
                {
                    Definition = new NotificationDefinition() { InterestId = args.InterestId, UpdaterId = args.UpdaterId },
                    TypeArgs = args.TypeArgs,
                    UpdatedAt = args.UpdateTime,
                    SizeBytes = args.Data.Length,
                    //encrypted?
                },
                Data = args.Data
            };
        }
    }
}

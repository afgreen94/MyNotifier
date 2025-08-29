using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Proxy
{


    //public class ProxyFileServerInterfacing : IProxyFileServerInterfacing
    //{

    //    private readonly IFileIOManager fileIOManager;
    //    private readonly IConfiguration configuration;
    //    private readonly ICallContext<ProxyFileServerInterfacing> callContext;

    //    public ProxyFileServerInterfacing(IFileIOManager fileIOManager,
    //                                      IConfiguration configuration,
    //                                      ICallContext<ProxyFileServerInterfacing> callContext)
    //    {
    //        this.fileIOManager = fileIOManager;
    //        this.configuration = configuration;
    //        this.callContext = callContext;
    //    }

    //    #region General
    //    public ValueTask<ICallResult> RetrieveMapFileAsync()
    //    {
    //        throw new NotImplementedException();
    //    }


    //    public ValueTask<ICallResult> DownloadUpdaterDllsAsync()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #endregion General

    //    #region IProxyFileServerNotifierPublisher

    //    private const string MetadataFilename = "metadata";
    //    private const string Datafilename = "data";

    //    public async ValueTask<ICallResult> WriteNotificationFilesAsync(Notification notification)
    //    {
    //        try
    //        {

    //            var notificationDirectoryPath = BuildNotificationDirectoryPath(notification);

    //            var createDirectoryResult = await fileIOManager.CreateDirectoryAsync(notificationDirectoryPath).ConfigureAwait(false);

    //            if (!createDirectoryResult.Success) return createDirectoryResult;

    //            var metadataJsonBytes = GetMetadataBytes(notification.Metadata);

    //            var writeNotificationMetadataFileTask = WriteNotificationFilesAsync(notificationDirectoryPath, MetadataFilename, metadataJsonBytes);
    //            var writeNotificationFileTask = WriteNotificationFilesAsync(notificationDirectoryPath, Datafilename, notification.Data);

    //            await Task.WhenAll(writeNotificationFileTask, writeNotificationMetadataFileTask).ConfigureAwait(false);

    //            if (!writeNotificationFileTask.IsCompletedSuccessfully) return BuildCallResultForFailedWriteTask(writeNotificationFileTask);
    //            if (!writeNotificationMetadataFileTask.IsCompletedSuccessfully) return BuildCallResultForFailedWriteTask(writeNotificationMetadataFileTask);

    //            return new CallResult();
    //        }
    //        catch (Exception ex) { return new CallResult(false, ex.Message); }
    //    }

    //    private async Task WriteNotificationFilesAsync(string directoryPath, string filename, byte[] data)
    //    {

    //        var createWriteStreamResult = fileIOManager.CreateWriteFileStream(directoryPath + filename, FileMode.CreateNew, false);

    //        if (!createWriteStreamResult.Success) throw new Exception(createWriteStreamResult.ErrorText); // fix this 

    //        if (data != null && data.Length != 0)
    //        {
    //            using var ws = createWriteStreamResult.Result;

    //            await ws.WriteAsync(data).ConfigureAwait(false);
    //        }
    //    }

    //    private string BuildNotificationDirectoryPath(Notification notification)
    //    {
    //        var sb = new StringBuilder();

    //        sb.Append(configuration.PublishDirectoryRoot);

    //        if (!configuration.PublishDirectoryRoot.EndsWith('/')) sb.Append('/');

    //        sb.Append(notification.Metadata.UpdatedAt.Ticks);
    //        sb.Append('_');
    //        sb.Append(notification.Metadata.Definition.Id);
    //        sb.Append('_');
    //        sb.Append(notification.Metadata.Definition.Name);
    //        sb.Append('/');

    //        return sb.ToString();
    //    }

    //    private byte[] GetMetadataBytes(NotificationMetadata metadata) => JsonSerializer.SerializeToUtf8Bytes(metadata);

    //    private ICallResult<PublishResult> BuildCallResultForFailedWriteTask(Task writeTask) => new CallResult<PublishResult>(false, writeTask.Exception != null ? writeTask.Exception.Message : "DEFAULT WRITE FAILURE EXCEPTION MESSAGE");

    //    #endregion IProxyFileServerNotifierPublisher

    //    #region IProxyFileServerNotifier

    //    public ValueTask<ICallResult<string>> SeekNewNotificationsAsync(string path)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public ValueTask<ICallResult> VerifyMetadataAndDataFilesExist(string notification)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public ValueTask<ICallResult<Notification>> ReadInNotificationAsync(string notification)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public ValueTask<ICallResult> DeleteNotificationAsync(string notification)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    #endregion IProxyFileServerNotifier


    //    public interface IConfiguration : IConfigurationWrapper
    //    {
    //        string PublishDirectoryRoot { get; }
    //    }
    //    public class Configuration : ConfigurationWrapper, IConfiguration
    //    {
    //        public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
    //        {
    //        }

    //        public string PublishDirectoryRoot => throw new NotImplementedException();
    //    }
    //}
}

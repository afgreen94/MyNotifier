using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Proxy;
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

        //must have all subservice dependencies in aggregate constructor //for now, provide default 
        //protected readonly INotificationFileSystemObjectTranslator translator = new DefaultTranslator(); 

        public virtual async Task<ICallResult<bool>> NotificationDirectoryExistsAsync() //Assert or Ensure ?? could make configurable 
        {
            try
            {
                if (!this.isInitialized) return BuildNotInitializedCallResult<bool>();

                var directoryExistsResult = await this.fileIOManager.DirectoryExistsAsync(this.configuration.NotificationsDirectoryName).ConfigureAwait(false); //could configure fileIOManager with notifications directory name 
                if (!directoryExistsResult.Success) return CallResult<bool>.BuildFailedCallResult(directoryExistsResult, "Failed to connect: {0}");

                return new CallResult<bool>(directoryExistsResult.Result);

            }
            catch (Exception ex) { return CallResult<bool>.FromException(ex); }
        }

        public virtual async Task<ICallResult<NotificationHeader[]>> RetrieveNotificationHeadersAsync()
        {
            try
            {
                if (!this.isInitialized) return BuildNotInitializedCallResult<NotificationHeader[]>();

                var getNotificationsResult = await this.fileIOManager.GetDirectoriesAsync(this.configuration.NotificationsDirectoryName).ConfigureAwait(false);
                if (!getNotificationsResult.Success) { return CallResult<NotificationHeader[]>.BuildFailedCallResult(getNotificationsResult, "Failed to retrieve notifications: {0}"); }
                var notificationDirectories = getNotificationsResult.Result;

                //fileIOManager will return fullpath, relative to root directory. could configure to root in Notifications/ folder, assuming this is not the case, parse directory names
                var notificationHeaders = new NotificationHeader[notificationDirectories.Length];

                for (int i = 0; i < notificationHeaders.Length; i++) notificationHeaders[i] = this.ToNotificationHeader(notificationDirectories[i]);  //this.translator.ToNotificationDescription(notificationDirectories[i], this.fileIOManagerWrapper.DirectorySeparator);  //delimiters must be configurable !!!   NotificationFolderObjectDescription.FromPath(notificationDirectories[i], '/', '_'); 

                return new CallResult<NotificationHeader[]>(notificationHeaders);

            }
            catch (Exception ex) { return CallResult<NotificationHeader[]>.FromException(ex); }
        }

        public virtual async Task<ICallResult<Notification>> ReadInNotificationAsync(NotificationHeader notificationHeader)
        {
            try
            {
                if (!this.isInitialized) return BuildNotInitializedCallResult<Notification>();

                //read all files 
                //assure metadata - data files match 
                //foreach pair, read both 
                //build notification 

                //var notificationFolderObject = new NotificationFolderObject() { Description = folderObjectDescription };

                var notificationFolderPath = this.fileIOManagerWrapper.BuildAppendedPath(this.paths.NotificationsFolder.Path, this.ToFolderName(notificationHeader));

                //var writeCompleteFilepath = this.fileIOManagerWrapper.BuildAppendedPath(notificationFolderPath, this.configuration.WriteCompleteSignalArgs.Name);
                var metadataFilepath = this.fileIOManagerWrapper.BuildAppendedPath(notificationFolderPath, this.configuration.MetadataFileName);
                var dataFilepath = this.fileIOManagerWrapper.BuildAppendedPath(notificationFolderPath, this.configuration.DataFileName);

                ////await write_complete signal 
                //if (this.configuration.WriteCompleteSignalArgs != null) //using write_complete signal scheme 
                //{
                //    var awaitWriteCompleteSignalResult = await this.AwaitWriteCompleteSignalAsync(notificationDescription, writeCompleteFilepath).ConfigureAwait(false);
                //    if (!awaitWriteCompleteSignalResult.Success) return CallResult<Notification>.BuildFailedCallResult(awaitWriteCompleteSignalResult, $"Failed to detect signal file reading notification with Id: {notificationDescription.Id}: {0}");

                //    //just delete here? , could be redundant since whole directory is deleted anyway, usually
                //    //also, shouldn't be deleted until after notification delivered, to account for intermediary errors 
                //    //finally, not necessarily using wcs scheme, so wcs logic later on must be implemented anyway 
                //    //or ignore later on
                //}

                //MAY BE UNNECESSARY 
                //var populateNotificationFileObjectDescriptionsResult = await this.PopulateNotificationFileDescriptionsAsync(notificationFolderObject).ConfigureAwait(false);
                //if (!populateNotificationFileObjectDescriptionsResult.Success) return CallResult<Notification>.BuildFailedCallResult(populateNotificationFileObjectDescriptionsResult, "Failed to populate notification folder object: {0}");

                ////now populated with descriptions and empty metadata/data fields
                ////match and fill fields

                //var createMetadataReadStreamResult = this.fileIOManager.CreateReadFileStream(metadataFilepath);
                //if (!createMetadataReadStreamResult.Success) { return CallResult<Notification>.BuildFailedCallResult(createMetadataReadStreamResult, $"Failed to create read stream for metadata for notification with Id: {notificationHeader.Id}: {{0}}"); }

                ////must decode from UTF8? //metadata is probably written as UTF8 bytes //DIFFERENT TYPES OF METADATA !!! 
                //string metadataJson;
                //using (var metadataReadStream = new StreamReader(createMetadataReadStreamResult.Result, this.defaultEncoding)) { metadataJson = await metadataReadStream.ReadToEndAsync().ConfigureAwait(false); }

                //var deserializeResult = this.DeserializedNotificationMetadata(metadataJson);
                //if (!deserializeResult.Success) return CallResult<Notification>.BuildFailedCallResult(deserializeResult, $"Failed to deserialize metadata for notification with Id: {notificationHeader.Id}: {{0}}");
                //var metadata = deserializeResult.Result;

                var metadataFileFoundResult = await this.AssertFileExistsWithRetryAsync(metadataFilepath, "Metadata").ConfigureAwait(false);
                if (!metadataFileFoundResult.Success) return CallResult<Notification>.BuildFailedCallResult(metadataFileFoundResult, $"Failed to locate metadata file for notification with Id: {notificationHeader.Id}: {{0}}");

                var createMetadataReadStreamResult = this.fileIOManager.CreateReadFileStream(metadataFilepath);
                if (!createMetadataReadStreamResult.Success) { return CallResult<Notification>.BuildFailedCallResult(createMetadataReadStreamResult, $"Failed to create read stream for metadata for notification with Id: {notificationHeader.Id}: {{0}}"); }

                //must decode from UTF8? //metadata is probably written as UTF8 bytes //DIFFERENT TYPES OF METADATA !!! 
                string metadataJson;
                using (var metadataReadStream = new StreamReader(createMetadataReadStreamResult.Result, this.defaultEncoding)) { metadataJson = await metadataReadStream.ReadToEndAsync().ConfigureAwait(false); }

                if (string.IsNullOrEmpty(metadataJson)) return new CallResult<Notification>(false, $"Failed to read metadata for notification with Id: {notificationHeader.Id}: json null or empty.");

                var deserializeResult = this.DeserializedNotificationMetadata(metadataJson, notificationHeader);
                if (!deserializeResult.Success) return CallResult<Notification>.BuildFailedCallResult(deserializeResult, $"Failed to deserialize metadata for notification with Id: {notificationHeader.Id}: {{0}}");
                var metadata = deserializeResult.Result;


                //APPLY DOWNLOAD CRITERIA FROM METADATA !!! 

                var createDataReadStreamResult = this.fileIOManager.CreateReadFileStream(dataFilepath);
                if (!createDataReadStreamResult.Success) { return CallResult<Notification>.BuildFailedCallResult(createDataReadStreamResult, $"Failed to create read stream for data for notification with Id: {notificationHeader.Id}: {{0}}"); }

                byte[] data;
                using (var dataReadStream = createDataReadStreamResult.Result)
                {
                    using var ms = new MemoryStream();
                    await dataReadStream.CopyToAsync(ms).ConfigureAwait(false);
                    data = ms.ToArray();
                }

                if (this.configuration.DeleteNotificationOnDelivered)
                {
                    var deleteResult = await this.fileIOManager.DeleteDirectoryAsync(notificationFolderPath).ConfigureAwait(false);
                    if (!deleteResult.Success) return CallResult<Notification>.BuildFailedCallResult(deleteResult, $"Failed to delete notification with Id: {notificationHeader.Id}: {{0}}");
                }
                //else if (this.configuration.WriteCompleteSignalArgs != null && this.configuration.WriteCompleteSignalArgs.DeleteWriteCompleteFileOnDelivered)
                //{
                //    var deleteWriteCompleteSignalFileResult = await this.fileIOManager.DeleteFileAsync(writeCompleteFilepath).ConfigureAwait(false);
                //    if (!deleteWriteCompleteSignalFileResult.Success) return CallResult<Notification>.BuildFailedCallResult(deleteWriteCompleteSignalFileResult, "Failed to delete write_complete signal file: {0}");
                //}

                return new CallResult<Notification>(new Notification() { Metadata = metadata, Data = data });
            }
            catch (Exception ex) { return new CallResult<Notification>(false, ex.Message); }
        }


        protected virtual Task CreateNotificationsDirectoryAsync() => throw new NotImplementedException();

        //private const string NotificationIdToken = "ID";
        //private const string UpdateTicksToken = "TICKS";
        //private const string NotificationTypeToken = "TYPE";
        //private const char FormatDelimiter = '_';
        //private readonly string notificationDirectoryNameFormat = $"{NotificationIdToken}{FormatDelimiter}{UpdateTicksToken}{FormatDelimiter}{NotificationTypeToken}";


        //protected virtual async Task<ICallResult> AwaitWriteCompleteSignalAsync(NotificationDescription notificationDescription, string writeCompleteFilePath)
        //{
        //    try
        //    {
        //        bool signalFileDetected = false;
        //        var retryDelaysEnumerator = this.configuration.WriteCompleteSignalArgs
        //                                                      .RetrySequenceDelaysMs
        //                                                      .GetEnumerator();

        //        while (retryDelaysEnumerator.MoveNext())
        //        {
        //            var fileExistsResult = await this.fileIOManager.FileExistsAsync(writeCompleteFilePath).ConfigureAwait(false);
        //            if (!fileExistsResult.Success) return CallResult.BuildFailedCallResult(fileExistsResult, "File exists calls failed: {0}");

        //            signalFileDetected = fileExistsResult.Result;

        //            if (signalFileDetected) return new CallResult();

        //            await Task.Delay((int)retryDelaysEnumerator.Current).ConfigureAwait(false);
        //        }

        //        return new CallResult(false, "Could not find write-complete signal file, configured delay sequence exhausted.");
        //    }
        //    catch (Exception ex) { return CallResult.FromException(ex); }
        //}

        protected virtual async Task<ICallResult> AssertFileExistsWithRetryAsync(string filePath, string semanticName)
        {
            try
            {
                ICallResult<bool> fileExistsResult = null;

                var retryDelaysEnumerator = this.configuration.RetrySequenceDelaysMs.GetEnumerator();
                while (retryDelaysEnumerator.MoveNext())
                {
                    fileExistsResult = await this.fileIOManager.FileExistsAsync(filePath).ConfigureAwait(false);
                    if (!fileExistsResult.Success) return CallResult.BuildFailedCallResult(fileExistsResult, $"File exists calls for {semanticName} file failed: {0}");
                    if (fileExistsResult.Result) break;

                    await Task.Delay((int)retryDelaysEnumerator.Current).ConfigureAwait(false);
                }

                if (fileExistsResult == null) return new CallResult(false, $"Failed to execute retry sequence in download attempt of {semanticName} file");
                if (!fileExistsResult.Result) return new CallResult(false, $"Could not find {semanticName} file, configured delay sequence exhausted.");

                return new CallResult();

            } catch(Exception ex) { return CallResult.FromException(ex); }
        }

        //protected virtual async Task<ICallResult<TModel>> DownloadAndDeserializeWithRetryAsync<TModel>(string filePath, string semanticName)
        //{
        //    try
        //    {
        //        ICallResult<bool> fileExistsResult = null;

        //        var retryDelaysEnumerator = this.configuration.RetrySequenceDelaysMs.GetEnumerator();
        //        while (retryDelaysEnumerator.MoveNext())
        //        {
        //            fileExistsResult = await this.fileIOManager.FileExistsAsync(filePath).ConfigureAwait(false);
        //            if (!fileExistsResult.Success) return CallResult<TModel>.BuildFailedCallResult(fileExistsResult, $"File exists calls for {semanticName} file failed: {0}");
        //            if (fileExistsResult.Result) break;

        //            await Task.Delay((int)retryDelaysEnumerator.Current).ConfigureAwait(false);
        //        }

        //        if (fileExistsResult == null) return new CallResult<TModel>(false, $"Failed to execute retry sequence in download attempt of {semanticName} file");
        //        if (!fileExistsResult.Result) return new CallResult<TModel>(false, $"Could not find {semanticName} file, configured delay sequence exhausted.");

        //        var readStreamResult = this.fileIOManager.CreateReadFileStream(filePath);
        //        if (!readStreamResult.Success) return CallResult<TModel>.BuildFailedCallResult(readStreamResult, $"Failed to create read stream for {semanticName} file: {{0}}");

        //        string json;
        //        using (var sr = new StreamReader(readStreamResult.Result)) { json = await sr.ReadToEndAsync().ConfigureAwait(false); }

        //        if (string.IsNullOrEmpty(json)) return new CallResult<TModel>(false, $"Invalid or empty json for {semanticName} file.");

        //        var model = JsonSerializer.Deserialize<TModel>(json);
        //        if (model == null) return new CallResult<TModel>(false, $"Failed to deserialize json for {semanticName} model.");

        //        return new CallResult<TModel>(model);
        //    } 
        //    catch(Exception ex) { return CallResult<TModel>.FromException(ex); }
        //}

        protected virtual ICallResult<NotificationMetadata> DeserializedNotificationMetadata(string metadataJson, NotificationHeader header) //need custom converter !!! For now use universal metadata model 
        {
            try
            {
                var model = JsonSerializer.Deserialize<NotificationMetadataModel>(metadataJson);
                if (model == null) return new CallResult<NotificationMetadata>(false, "Invalid metadata json.");

                NotificationMetadata metadata = new()
                {
                    DataTypeArgs = new DataTypeArgs() { DataType = Contracts.EnumStringMaps.GetNotificationDataType(model.DataTypeArgs.DataType), Description = model.DataTypeArgs.Description },
                    Encrypted = model.Encrypted,
                    SizeBytes = model.SizeBytes
                };

                switch (header.Type)
                {
                    case NotificationType.Update:
                        metadata.Description = GetUpdateNotificationMetadata(model.Description);
                        break;
                    case NotificationType.CommandResult:
                        metadata.Description = GetCommandResultNotificationMetadata(model.Description);
                        break;
                }

                metadata.Description.Header = header;
                metadata.Description.PublishedAt = model.Description.PublishedAt;
                metadata.Description.UpdatedAt = model.Description.UpdatedAt;
                metadata.Description.PublishedTo = model.Description.PublishedTo;

                return new CallResult<NotificationMetadata>(metadata);
            }
            catch (Exception ex) { return CallResult<NotificationMetadata>.FromException(ex); }
        }

        private static UpdateNotificationDescription GetUpdateNotificationMetadata(NotificationDescriptionModel model) => new()
        {
            InterestDefinitionId = model.InterestDefinitionId,
            InterestId = model.InterestId,
            EventModuleDefinitionId = model.EventModuleDefinitionId,
            EventModuleId = model.EventModuleId,
            UpdaterDefinitionId = model.UpdaterDefinitionId,
            UpdaterId = model.UpdaterId
        };

        private static CommandResultNotificationDescription GetCommandResultNotificationMetadata(NotificationDescriptionModel model) => new()
        {
            CommandNotificationId = model.CommandNotificationId
        };

        //if this call is made between when directory is created and files are written, get files call will return bad result. Could add retry to account for wait
        //alternatively, abandon folderNotificationSignal scheme and just use notification files. this would work because metadata file would always be light, so write time would be negligible 
        //alternatively, publisher could create WRITE_COMPLETE signal, which notifier destroys thereafter 
        //for now, use retry 
        //Eventually, implement LargeFileDownloadOptions from Metadata file 
        //protected async Task<ICallResult> PopulateNotificationFileDescriptionsAsync(NotificationDescription notificationDescription, string notificationFolderPath)
        //{
        //    try
        //    {
        //        //var expectedFileCount = notificationFolderObject.Description.Type == NotificationType.Update ? DefaultExpectedUpdateNotificationFileCount : DefaultExpectedNotificationFileCount;

        //        //here we are expected publication will succeed, and give publisher time to write files 

        //        int[] retryDelaysMs = []; //allow for file write 

        //        var getNotificationFilesResult = await this.fileIOManager.GetFilesAsync(notificationFolderPath).ConfigureAwait(false);
        //        if (!getNotificationFilesResult.Success) return CallResult<Notification>.BuildFailedCallResult(getNotificationFilesResult, $"Failed to find notification files for notification with Id: {notificationDescription.Id}: {{0}}");

        //        var notificationFilePaths = getNotificationFilesResult.Result;

        //        foreach (var path in notificationFilePaths)
        //        {
        //            //unfortunately, using wcs scheme, with logic in current form, wc file must be manually ignored. this can be refactored later 
        //            if (this.configuration.WriteCompleteSignalArgs != null && path.Split(this.fileIOManagerWrapper.DirectorySeparator)[^1] == this.configuration.WriteCompleteSignalArgs.Name) continue;

        //            var notificationFileObjectDescription = this.translator.ToFileObjectDescription(path, this.fileIOManagerWrapper.DirectorySeparator);  //NotificationFileObjectDescription.FromPath(notificationFilePaths[i], '/', '_');

        //            if (notificationFileObjectDescription.FileType == NotificationFileType.Metadata) notificationFolderObject.MetadataFileObject = new NotificationFileObject() { Description = notificationFileObjectDescription };
        //            else if (notificationFileObjectDescription.FileType == NotificationFileType.Data) notificationFolderObject.DataFileObject = new NotificationFileObject() { Description = notificationFileObjectDescription };
        //            else { return new CallResult(false, "Encountered unknown notification file type"); } // should not happen 
        //        }

        //        //for non-updates 
        //        //if(notificationFolderObject.DataFileObject == null || ((expectedFileCount == DefaultExpectedUpdateNotificationFileCount) && notificationFolderObject.MetadataFileObject == null))

        //        if (notificationFolderObject.MetadataFileObject == null || notificationFolderObject.DataFileObject == null) return new CallResult(false, $"Missing notification file for notification: {notificationFolderObject.Description.Name}");

        //        return new CallResult();
        //    }
        //    catch (Exception ex) { return CallResult.FromException(ex); }
        //}

        ////to expedite write_complete method 
        //private async Task<ICallResult<NotificationFileObject[]>> RetrieveNotificationFolderContentsAsync(string notificationFolderPath)
        //{
        //    var retryDelayEnumerator = new int[0].GetEnumerator();

        //    NotificationFileObject metadataFileObject = null;
        //    NotificationFileObject dataFileObject = null;

        //    while ((metadataFileObject == null || dataFileObject == null) && retryDelayEnumerator.MoveNext())
        //    {
        //        var getNotificationFilesResult = await this.fileIOManager.GetFilesAsync(notificationFolderPath).ConfigureAwait(false);
        //        if (!getNotificationFilesResult.Success) return CallResult<NotificationFileObject[]>.BuildFailedCallResult(getNotificationFilesResult, $"Get notification files called failed for notification {notificationFolderPath}: {{0}}");


        //    }


        //    return new CallResult<NotificationFileObject[]>();
        //}
    }
}

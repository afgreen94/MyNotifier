using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Notifications;

namespace MyNotifier.Notifiers
{
    public partial class FileNotifier 
    {
        protected class FileNotifierHelper  //USING WRITE_COMPLETE SIGNAL SCHEME !!! EVENTUALLY WILL WRAP NATURALLY INTO NOTIFICATION POLLING, NO NEED FOR SIGNAL 
        {
            //private const int DefaultExpectedNotificationFileCount = 1;  //for commands and exceptions 
            //private const int DefaultExpectedUpdateNotificationFileCount = 3; //metadata file, data file, and signal file: hardcoded for now 

            private readonly IFileIOManager fileIOManager;
            private readonly INotificationFileSystemObjectTranslator translator;
            private readonly IConfiguration configuration;

            private readonly NotificationType targetNotificationTypeMask;

            private IFileIOManager.IWrapper wrapper;

            private Encoding defaultEncoding = Encoding.UTF8; //make configurable 

            //cache
            private DateTime nextClearCache = DateTime.UtcNow;
            private HashSet<string> processedNotificationNamesCache = []; //names should be unique, could override FolderObj GetHashCode() and cache full objs. using names may be lighter, but making the assumption names are unique(should be true)
            private long lastNotificationTicks = 0L; //can use lastNotificationTicks to exclude redundant notifications and substitute for cache. however, there is possibilty of notification publication delay causing skips. maybe can only safely use cache //for now, using only cache //should work actually! 

            public FileNotifierHelper(IFileIOManager fileIOManager, INotificationFileSystemObjectTranslator translator, IConfiguration configuration)
            {
                this.fileIOManager = fileIOManager;
                this.wrapper = new FileIOManager.FileIOManager.Wrapper(fileIOManager);

                this.translator = translator;

                this.targetNotificationTypeMask = configuration.AllowedNotificationTypeArgs.ToNotificationTypeMask();

                this.configuration = configuration;
            }

            public async Task<ICallResult<Notification[]>> RetrieveNewNotificationsAsync()
            {
                try
                {
                    var retrieveNotificationsResult = await this.RetrieveNotificationDirectoriesAsync().ConfigureAwait(false);
                    if (!retrieveNotificationsResult.Success) CallResult<Notification[]>.BuildFailedCallResult(retrieveNotificationsResult, "Failed to retrieve notifications: {0}"); //cast to be safe ? 

                    var notificationFolderObjectDescriptions = retrieveNotificationsResult.Result;
                    var latestTicks = this.lastNotificationTicks;

                    var notifications = new Notification[notificationFolderObjectDescriptions.Length];

                    for (int i = 0; i < notifications.Length; i++)
                    {
                        var notificationFolderObjectDescription = notificationFolderObjectDescriptions[i];

                        if (this.TryExclude(notificationFolderObjectDescription)) continue;

                        var ReadInNotificationResult = await this.ReadInNotificationAsync(notificationFolderObjectDescription).ConfigureAwait(false);
                        //how to handle, for now fail all.
                        if (!ReadInNotificationResult.Success) return CallResult<Notification[]>.BuildFailedCallResult(ReadInNotificationResult, $"Failed to read in notification for {notificationFolderObjectDescription.Name}: {{0}}");

                        notifications[i] = ReadInNotificationResult.Result;

                        if (notificationFolderObjectDescription.Ticks > latestTicks) latestTicks = notificationFolderObjectDescription.Ticks;

                        var nowTime = DateTime.UtcNow;
                        if (this.nextClearCache > nowTime)
                        {
                            this.processedNotificationNamesCache.Clear();
                            this.nextClearCache = nowTime + this.configuration.ClearCacheInterval;
                        }

                        this.processedNotificationNamesCache.Add(notificationFolderObjectDescription.Name);
                    }

                    this.lastNotificationTicks = latestTicks;

                    return new CallResult<Notification[]>(notifications);
                }
                catch (Exception ex) { return CallResult<Notification[]>.FromException(ex); }
            }

            private async Task<ICallResult<NotificationFolderObjectDescription[]>> RetrieveNotificationDirectoriesAsync()
            {
                try
                {
                    var getNotificationsResult = await this.fileIOManager.GetDirectoriesAsync(this.configuration.NotificationsDirectoryName).ConfigureAwait(false);
                    if (!getNotificationsResult.Success) { return CallResult<NotificationFolderObjectDescription[]>.BuildFailedCallResult(getNotificationsResult, "Failed to retrieve notification folders: {0}"); }
                    var notificationDirectories = getNotificationsResult.Result;

                    //fileIOManager will return fullpath, relative to root directory. could configure to root in Notifications/ folder, assuming this is not the case, parse directory names
                    var notificationFolderObjectDescriptions = new NotificationFolderObjectDescription[notificationDirectories.Length];

                    for (int i = 0; i < notificationFolderObjectDescriptions.Length; i++) notificationFolderObjectDescriptions[i] = this.translator.ToFolderObjectDescription(notificationDirectories[i], this.wrapper.DirectorySeparator);  //delimiters must be configurable !!!   NotificationFolderObjectDescription.FromPath(notificationDirectories[i], '/', '_'); 

                    return new CallResult<NotificationFolderObjectDescription[]>(notificationFolderObjectDescriptions);

                }
                catch (Exception ex) { return CallResult<NotificationFolderObjectDescription[]>.FromException(ex); }

            }

            private async Task<ICallResult<Notification>> ReadInNotificationAsync(NotificationFolderObjectDescription folderObjectDescription)
            {
                try
                {

                    //read all files 
                    //assure metadata - data files match 
                    //foreach pair, read both 
                    //build notification 

                    var notificationFolderObject = new NotificationFolderObject() { Description = folderObjectDescription };

                    //await write_complete signal 
                    if (this.configuration.WriteCompleteSignalArgs != null) //using write_complete signal scheme 
                    {
                        var awaitWriteCompleteSignalResult = await this.AwaitWriteCompleteSignalAsync(notificationFolderObject).ConfigureAwait(false);
                        if (!awaitWriteCompleteSignalResult.Success) return CallResult<Notification>.BuildFailedCallResult(awaitWriteCompleteSignalResult, "Failed to detect signal file: {0}");

                        //just delete here , could be redundant since whole directory is deleted anyway, usually
                        //also, shouldn't be deleted until after notification delivered, to account for intermediary errors 
                        //finally, not necessarily using wcs scheme, so wcs logic later on must be implemented anyway 
                        //or ignore later on
                    }

                    var populateNotificationFileObjectDescriptionsResult = await this.PopulateNotificationFileDescriptionsAsync(notificationFolderObject).ConfigureAwait(false);
                    if (!populateNotificationFileObjectDescriptionsResult.Success) return CallResult<Notification>.BuildFailedCallResult(populateNotificationFileObjectDescriptionsResult, "Failed to populate notification folder object: {0}");

                    //now populated with descriptions and empty metadata/data fields
                    //match and fill fields 

                    var createMetadataReadStreamResult = this.fileIOManager.CreateReadFileStream(notificationFolderObject.MetadataFileObject.Description.Path);
                    if (!createMetadataReadStreamResult.Success) { return new CallResult<Notification>(false, $"Failed to create read stream for metadata for notification: {notificationFolderObject.Description.Name}"); }


                    //must decode from UTF8? //metadata is probably written as UTF8 bytes 
                    NotificationMetadata metadata;
                    using (var metadataReadStream = new StreamReader(createMetadataReadStreamResult.Result, this.defaultEncoding))
                    {
                        var metadataJson = await metadataReadStream.ReadToEndAsync().ConfigureAwait(false);
                        var metadataNullable = JsonSerializer.Deserialize<NotificationMetadata>(metadataJson);

                        if (metadataNullable != null) metadata = metadataNullable;
                        else return new CallResult<Notification>(false, "Invalid metadata");
                    }

                    var createDataReadStreamResult = this.fileIOManager.CreateReadFileStream(notificationFolderObject.DataFileObject.Description.Path);
                    if (!createDataReadStreamResult.Success) { return new CallResult<Notification>(false, $"Failed to create read stream for data for notification: {notificationFolderObject.Description.Name}"); }

                    byte[] data;
                    using (var dataReadStream = createDataReadStreamResult.Result)
                    {
                        using var ms = new MemoryStream();
                        await dataReadStream.CopyToAsync(ms).ConfigureAwait(false);
                        data = ms.ToArray();
                    }

                    if (this.configuration.DeleteNotificationOnDelivered)
                    {
                        var deleteResult = await this.fileIOManager.DeleteDirectoryAsync(notificationFolderObject.Description.Path).ConfigureAwait(false);
                        if (!deleteResult.Success) return new CallResult<Notification>(false, $"Failed to delete notification: {notificationFolderObject.Description.Name}");
                    }
                    else if (this.configuration.WriteCompleteSignalArgs != null && this.configuration.WriteCompleteSignalArgs.DeleteWriteCompleteFileOnDelivered)
                    {
                        var deleteWriteCompleteSignalFileResult = await this.fileIOManager.DeleteFileAsync(this.wrapper.BuildAppendedPath(notificationFolderObject.Description.Path, this.configuration.WriteCompleteSignalArgs.Name)).ConfigureAwait(false);   
                        if (!deleteWriteCompleteSignalFileResult.Success) return CallResult<Notification>.BuildFailedCallResult(deleteWriteCompleteSignalFileResult, "Failed to delete write_complete signal file: {0}");
                    }

                    return new CallResult<Notification>(new Notification() { Metadata = metadata, Data = data });
                }
                catch (Exception ex) { return new CallResult<Notification>(false, ex.Message); }
            }

            private async Task<ICallResult> AwaitWriteCompleteSignalAsync(NotificationFolderObject notificationFolderObject)
            {
                try
                {
                    bool signalFileDetected = false;
                    var retryDelaysEnumerator = this.configuration.WriteCompleteSignalArgs.RetrySequenceDelaysMs.GetEnumerator();

                    while (retryDelaysEnumerator.MoveNext())
                    {
                        var fileExistsResult = await this.fileIOManager.FileExistsAsync(this.wrapper.BuildAppendedPath(notificationFolderObject.Description.Path, this.configuration.WriteCompleteSignalArgs.Name)).ConfigureAwait(false);
                        if (!fileExistsResult.Success) return CallResult.BuildFailedCallResult(fileExistsResult, "File exists calls failed: {0}");

                        signalFileDetected = fileExistsResult.Result;

                        if (signalFileDetected) return new CallResult();

                        await Task.Delay((int)retryDelaysEnumerator.Current).ConfigureAwait(false);
                    }

                    return new CallResult(false, "Could not find signal file, configured delay sequence exhausted.");
                }
                catch (Exception ex) { return CallResult.FromException(ex); }
            }

            //if this call is made between when directory is created and files are written, get files call will return bad result. Could add retry to account for wait
            //alternatively, abandon folderNotificationSignal scheme and just use notification files. this would work because metadata file would always be light, so write time would be negligible 
            //alternatively, publisher could create WRITE_COMPLETE signal, which notifier destroys thereafter 
            //for now, use retry 
            private async Task<ICallResult> PopulateNotificationFileDescriptionsAsync(NotificationFolderObject notificationFolderObject)
            {
                try
                {
                    //var expectedFileCount = notificationFolderObject.Description.Type == NotificationType.Update ? DefaultExpectedUpdateNotificationFileCount : DefaultExpectedNotificationFileCount;

                    //here we are expected publication will succeed, and give publisher time to write files 

                    int[] retryDelaysMs = []; //allow for file write 

                    var getNotificationFilesResult = await this.fileIOManager.GetFilesAsync(notificationFolderObject.Description.Path).ConfigureAwait(false);
                    if (!getNotificationFilesResult.Success) return CallResult<Notification>.BuildFailedCallResult(getNotificationFilesResult, $"Failed to find notification files for notification {notificationFolderObject.Description.Name}: {{0}}");

                    var notificationFilePaths = getNotificationFilesResult.Result;

                    foreach (var path in notificationFilePaths)
                    {
                        //unfortunately, using wcs scheme, with logic in current form, wc file must be manually ignored. this can be refactored later 
                        if (this.configuration.WriteCompleteSignalArgs != null && path.Split(this.wrapper.DirectorySeparator)[^1] == this.configuration.WriteCompleteSignalArgs.Name) continue;

                        var notificationFileObjectDescription = this.translator.ToFileObjectDescription(path, this.wrapper.DirectorySeparator);  //NotificationFileObjectDescription.FromPath(notificationFilePaths[i], '/', '_');

                        if (notificationFileObjectDescription.FileType == NotificationFileType.Metadata) notificationFolderObject.MetadataFileObject = new NotificationFileObject() { Description = notificationFileObjectDescription };
                        else if (notificationFileObjectDescription.FileType == NotificationFileType.Data) notificationFolderObject.DataFileObject = new NotificationFileObject() { Description = notificationFileObjectDescription };
                        else { return new CallResult(false, "Encountered unknown notification file type"); } // should not happen 
                    }

                    //for non-updates 
                    //if(notificationFolderObject.DataFileObject == null || ((expectedFileCount == DefaultExpectedUpdateNotificationFileCount) && notificationFolderObject.MetadataFileObject == null))

                    if (notificationFolderObject.MetadataFileObject == null || notificationFolderObject.DataFileObject == null) return new CallResult(false, $"Missing notification file for notification: {notificationFolderObject.Description.Name}");

                    return new CallResult();
                }
                catch (Exception ex) { return CallResult.FromException(ex); }
            }

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

            private bool TryExclude(NotificationFolderObjectDescription folderObject) => this.IsExcludedNotificationType(folderObject) || this.IsAlreadyProcessedNotification(folderObject);
            private bool IsExcludedNotificationType(NotificationFolderObjectDescription folderObject) => (this.targetNotificationTypeMask & folderObject.Type) == 0;
            private bool IsAlreadyProcessedNotification(NotificationFolderObjectDescription folderObject) => this.processedNotificationNamesCache.Contains(folderObject.Name);

        }
    }
}

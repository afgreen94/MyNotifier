using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util;
using System.CodeDom.Compiler;
using Google.Apis.Requests;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using MyNotifier.Base.IO;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace MyNotifier.FileIOManager
{
    public class GoogleDriveFileIOManager : FileIOManager, IGoogleDriveFileIOManager
    {
        private new readonly IConfiguration configuration;

        private bool isInitialized = false;
        private DriveService client;

        public GoogleDriveFileIOManager(IConfiguration configuration, ICallContext<GoogleDriveFileIOManager> callContext) : base(configuration, callContext) { this.configuration = configuration; }

        protected override async ValueTask InitializeCoreAsync()
        {
            var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
            {
                ClientId = this.configuration.ClientId,
                ClientSecret = this.configuration.ClientSecret
            },
            [DriveService.Scope.Drive],
            "user",
            default).ConfigureAwait(false);

            this.client = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credentials });
        }

        protected override async Task<bool> FileExistsCoreAsync(string filePath)
        {
            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var folderPath);

            var folderId = await this.GetFolderIdAsync(folderPath).ConfigureAwait(false);

            var listRequest = this.client.Files.List();
            listRequest.Q = $"name='{fileName}' and '{folderId}' in parents and mimeType!='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            return result.Files.Count == 1;
        }

        //private async Task CopyFileCoreAsync(string fromFilePath, string toFilePath)
        //{
        //    this.GetLeafNameAndRootPathForPath(fromFilePath, out var fromFileName, out var fromFileDirectoryPath);
        //    this.GetLeafNameAndRootPathForPath(toFilePath, out var toFileName, out var toFileDirectoryPath);

        //    var fromfileId = await this.GetFileIdAsync(fromFilePath).ConfigureAwait(false);
        //    var toFolderId = await this.GetFolderIdAsync(toFileDirectoryPath).ConfigureAwait(false);

        //    var fromFile = await this.client.Files.Get(fromfileId).ExecuteAsync().ConfigureAwait(false);

        //    var copyRequest = this.client.Files.Copy(fromFile, fromFile.Id);

        //    await copyRequest.ExecuteAsync();

        //}

        protected override async Task<bool> DirectoryExistsCoreAsync(string directoryPath)
        {

            this.GetLeafNameAndRootPathForPath(directoryPath, out var directoryName, out var rootPath);

            var parentFolderId = await this.GetFolderIdAsync(rootPath).ConfigureAwait(false);

            //if (parentFolderId == "root") directoryName += '/'; //quirk with googledrive, children of root '/' 

            var listRequest = this.client.Files.List();
            listRequest.Q = $"name='{directoryName}' and '{parentFolderId}' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            return result.Files.Count == 1;
        }

        protected override async Task<string[]> GetFilesCoreAsync(string directoryPath = "/", string searchPattern = "") //search pattern not implemented 
        {
            if (!string.IsNullOrEmpty(searchPattern)) throw new Exception("Search pattern functionality not implemented");

            var directoryId = await this.GetFolderIdAsync(directoryPath).ConfigureAwait(false);

            var listRequest = this.client.Files.List();
            listRequest.Q = $"'{directoryId}' in parents and mimeType!='application/vnd.google-apps.folder' and trashed=false"; 

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            //add to cache? 

            //api will only return file names. conventionally, getFiles() should give fullpaths. must append fullpaths manually 
            return this.BuildFullPathsForGetResult(directoryPath, result);
        }

        protected override async Task CreateFileCoreAsync(string filePath)
        {
            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var folderPath);

            var directoryId = await this.GetFolderIdAsync(folderPath).ConfigureAwait(false);

            var createRequest = this.client.Files.Create(new Google.Apis.Drive.v3.Data.File() { Name = fileName, Parents = [directoryId] });
            
            var result = await createRequest.ExecuteAsync().ConfigureAwait(false);

            this.fileIdCache.Add(filePath, result.Id);
        }

        protected override async Task DeleteFileCoreAsync(string filePath)
        {
            var fileId = await this.GetFileIdAsync(filePath).ConfigureAwait(false);

            var deleteRequest = this.client.Files.Delete(fileId);
            await deleteRequest.ExecuteAsync().ConfigureAwait(false); //validate result ?

            this.fileIdCache.Remove(filePath);
        }

        private readonly long defaultUploadWaitDurationTicks = new TimeSpan(0, 1, 0).Ticks; //by default all uploads have 1 minute to complete, this will need to be configurable 
        protected override async Task UploadFileFromCoreAsync(string filePath, Stream sourceStream, bool canOverwrite = false)
        {
            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var folderPath);

            var directoryId = await this.GetFolderIdAsync(folderPath).ConfigureAwait(false);

            var createRequest = this.client.Files.Create(new Google.Apis.Drive.v3.Data.File() { Name = fileName, Parents = [directoryId] }, sourceStream, "application/unknown");
            var result = await createRequest.UploadAsync().ConfigureAwait(false);

            var startTimeTicks = DateTime.UtcNow.Ticks;
            while (result.Status != Google.Apis.Upload.UploadStatus.Uploading && (DateTime.UtcNow.Ticks - startTimeTicks > this.defaultUploadWaitDurationTicks)) ;  //could hang, assigned max duration condition 

            if (result.Status != Google.Apis.Upload.UploadStatus.Completed) throw new Exception($"Upload failed: {result.Exception.Message}");
        }

        protected override async Task DownloadFileToCoreAsync(string filePath, Stream destinationStream)
        {
            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var folderName);

            var fileId = await this.GetFileIdAsync(filePath).ConfigureAwait(false);

            var downloadRequest = this.client.Files.Download(fileId);
            var resultStream = await downloadRequest.ExecuteAsStreamAsync().ConfigureAwait(false);

            await resultStream.CopyToAsync(destinationStream).ConfigureAwait(false);
        }

        //not really live streaming, downloads whole file and transfers to memory stream 
        protected override Stream CreateReadFileStreamCore(string filePath) //problematic
        {
            var getFileIdTask = this.GetFileIdAsync(filePath);
            getFileIdTask.Wait();

            var fileId = getFileIdTask.Result;

            var ms = new MemoryStream();

            this.client.Files.Get(fileId).Download(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return ms;

            //var transferBuffer = new TransferBuffer();

            //this.client.Files.Get(fileId).DownloadAsync(new TransferBufferWriterStream(transferBuffer));

            //return new TransferBufferReaderStream(transferBuffer);
        }

        //can only support creating new files/overwriting exitsing files 
        //method has problems with creating duplicate files, updating existing files ( tbwritestream cannot seek? )
        //for now, disabling update functionality
        protected override Stream CreateWriteFileStreamCore(string filePath, FileMode fileMode) //problematic
        {
            //for now 
            //MANDATE CREATE NEW FILE 
                    
            if (fileMode == FileMode.Create || fileMode == FileMode.CreateNew)
            {
                this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var directoryPath);

                var getDirectoryIdTask = this.GetFolderIdAsync(directoryPath);
                getDirectoryIdTask.Wait();

                var directoryId = getDirectoryIdTask.Result;

                var file = new Google.Apis.Drive.v3.Data.File() { Name = fileName, Parents = [directoryId] };

                var transferBuffer = new TransferBuffer();

                this.client.Files.Create(file, new TransferBufferReaderStream(transferBuffer), "application/unknown").UploadAsync();

                return new TransferBufferWriterStream(transferBuffer);
            }
            else if (fileMode == FileMode.Open || fileMode == FileMode.Append)
            {
                throw new Exception("Method does not support updating existing files");

                //var getFileIdTask = this.GetFileIdAsync(filePath);
                //getFileIdTask.Wait();

                //var fileId = getFileIdTask.Result;

                //var getRequest = this.client.Files.Get(fileId);
                //var file = getRequest.Execute();

                //var transferBuffer = new TransferBuffer();

                ////this has to be dispatched to background for stream to be available in foreground ?? there should be a better way of doing this
                //this.client.Files.Update(file, fileId, new TransferBufferReaderStream(transferBuffer), "application/unknown").UploadAsync();

                //return new TransferBufferWriterStream(transferBuffer);
            }
            else throw new Exception("Unsupported FileMode");
        }

        protected override async Task<string[]> GetDirectoriesCoreAsync(string directoryPath = "/", string searchPattern = "")
        {
            if (!string.IsNullOrEmpty(searchPattern)) throw new Exception("Search pattern functionality not implemented");

            var parentFolderId = await this.GetFolderIdAsync(directoryPath).ConfigureAwait(false);

            var listRequest = this.client.Files.List();
            listRequest.Q = $"'{parentFolderId}' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            //same as with getFiles()
            return this.BuildFullPathsForGetResult(directoryPath, result);
        }

        protected override async Task CreateDirectoryCoreAsync(string directoryPath)
        {
            this.GetLeafNameAndRootPathForPath(directoryPath, out var directoryName, out var rootPath);

            var parentFolderId = await this.GetFolderIdAsync(rootPath).ConfigureAwait(false);

            var createRequest = this.client.Files.Create(new Google.Apis.Drive.v3.Data.File() { Name = directoryName, Parents = [parentFolderId], MimeType = "application/vnd.google-apps.folder" });
            var result = await createRequest.ExecuteAsync().ConfigureAwait(false);

            this.folderIdCache.Add(directoryPath, result.Id);
        }

        protected override async Task DeleteDirectoryCoreAsync(string directoryPath)
        {
            var folderId = await this.GetFolderIdAsync(directoryPath).ConfigureAwait(false);

            var deleteRequest = this.client.Files.Delete(folderId);
            await deleteRequest.ExecuteAsync().ConfigureAwait(false);

            this.folderIdCache.Remove(directoryPath);
        }

        //caches to track known file and folder ids @ given paths 
        //must be updated as file system changes 
        protected IDictionary<string, string> fileIdCache = new Dictionary<string, string>();
        protected IDictionary<string, string> folderIdCache = new Dictionary<string, string>() { { "", "root" }, { "/", "root" } };

        protected async Task<string> GetFileIdAsync(string filePath)
        {
            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var folderPath);

            var folderId = await this.GetFolderIdAsync(folderPath).ConfigureAwait(false);

            var listRequest = this.client.Files.List();
            listRequest.Q = $"name='{fileName}' and '{folderId}' in parents and mimeType!='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            var fileId = result.Files[0].Id;

            this.fileIdCache[filePath] = fileId;

            return fileId;
        }

        protected async Task<string> GetFileIdAsync(string filePath, string parentFolderId)
        {
            if (this.fileIdCache.TryGetValue(filePath, out var cachedId)) return cachedId;

            this.GetLeafNameAndRootPathForPath(filePath, out var fileName, out var path);

            var listRequest = this.client.Files.List();
            listRequest.Q = $"name='{fileName}' and '{parentFolderId}' in parents and mimeType!='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            var fileId = result.Files[0].Id;

            this.fileIdCache[filePath] = fileId;

            return fileId;
        }

        protected async Task<string> GetFolderIdAsync(string folderPath)
        {

            if(this.folderIdCache.TryGetValue(folderPath, out var cachedId)) return cachedId;

            var parts = folderPath.Split('/');

            var parentFolderId = "root";
            var folderId = "";

            for (int i = 0; i < parts.Length; i++) 
            {
                //var folder = i == 0 ? parts[i] + '/' : parts[i]; //flaw with drive api, first child folder will not be found without trailing /, subsequent folders will not be found with it //this actually may not be true 

                var folder = parts[i];

                folderId = await this.GetFolderIdAsync(folder, parentFolderId).ConfigureAwait(false);

                this.folderIdCache[string.Join('/', new ArraySegment<string>(parts, 0, i + 1))] = folderId;

                parentFolderId = folderId;
            }

            return folderId;
        }

        protected async Task<string> GetFolderIdAsync(string folderName, string parentFolderId)
        {
            var listRequest = this.client.Files.List();
            listRequest.Q = $"name='{folderName}' and '{parentFolderId}' in parents and mimeType='application/vnd.google-apps.folder' and trashed=false";

            var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

            return result.Files[0].Id;
        }

        protected string[] BuildFullPathsForGetResult(string directoryPath, Google.Apis.Drive.v3.Data.FileList getResult) => getResult.Files.Select(c => new StringBuilder(directoryPath)
                                                                                                                                            .Append(this.DirectorySeparator)
                                                                                                                                            .Append(c.Name)
                                                                                                                                            .ToString())
                                                                                                                                            .ToArray();



        //protected async Task<string> GetParentFolderId(string folderPath)
        //{
        //    var parts = folderPath.Split('/');

        //    if (parts.Length == 0) throw new Exception("Invalid path");
        //    if (parts.Length == 1) return "root";

        //    var listRequest = this.client.Files.List();
        //    listRequest.Q = "";

        //    var result = await listRequest.ExecuteAsync().ConfigureAwait(false);

        //    return result.Files[0].Id;
        //}

        public new interface IConfiguration : FileIOManager.IConfiguration 
        {
            string ClientId { get; }
            string ClientSecret { get; }
        }
        public new class Configuration : FileIOManager.Configuration, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }

            public string ClientId => this.innerConfiguration.GetValue<string>("ClientId");
            public string ClientSecret => this.innerConfiguration.GetValue<string>("ClientSecret");
        }

        //TO BE IMPLEMENTED IN GDFIOM
        //should only need one map 
        //maybe need two, cannot distinguish between files and folders without implementing naming convention to that end. drive allows child folder and files to share names. either need 2 caches or naming convention 
        //threadsafe? //semaphore hits take time ...
        protected class Cache
        {
            private readonly SemaphoreSlim semaphore = new(1, 1);
            private readonly IDictionary<string, string> pathToIdMap = new Dictionary<string, string>();

            public string this[string path]
            {
                get => this.pathToIdMap[path];
                set
                {
                    try
                    {
                        this.semaphore.Wait();
                        this.pathToIdMap[path] = value;
                    }
                    finally { this.semaphore.Release(); }
                }
            }

            public bool TryGetIdForPath(string path, out string id) => this.pathToIdMap.TryGetValue(path, out id);
            public async Task AddIdForPathAsync(string path, string id)
            {
                try
                {
                    await this.semaphore.WaitAsync().ConfigureAwait(false);
                    this.pathToIdMap.Add(path, id);
                }
                finally { this.semaphore.Release(); }
            }
            public async Task<bool> RemoveIdForPathAsync(string path)
            {
                try
                {
                    await this.semaphore.WaitAsync().ConfigureAwait(false);
                    return this.pathToIdMap.Remove(path);
                }
                finally { this.semaphore.Release(); }
            }
        }

        //in case of transient GoogleDriveIOManager, static 'cacheSingleton' allows cache to be shared across instances. 
        //cache class protected to not be exposed to other serviceProvider callers. this is an interesting idea actually...
        protected readonly static Cache cacheSingleton = new();
    }


    public interface IGoogleDriveFileIOManager : IFileIOManager { }
}

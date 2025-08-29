using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Base;

namespace MyNotifier.FileIOManager
{
    //implement access control checks 
    public class LocalDriveFileIOManager : FileIOManager, ILocalDriveFileIOManager
    {

        private const char NativeDirectorySeparator = '\\';

        public LocalDriveFileIOManager(IConfiguration configuration, ICallContext<LocalDriveFileIOManager> callContext) : base(configuration, callContext) { }

        protected override Task<bool> FileExistsCoreAsync(string filePath) => Task.FromResult(File.Exists(filePath));
        protected override Task<string[]> GetFilesCoreAsync(string directoryPath = "/", string searchPattern = "")
        {
            var getFiles = Directory.GetFiles(directoryPath, searchPattern);
            this.ReplaceNativeDirectorySeparatorInPaths(getFiles);
            return Task.FromResult(getFiles);
        }
        protected override Task CreateFileCoreAsync(string filePath)
        {
            var handle = File.Create(filePath);
            handle.Dispose(); //dispose to close file handle

            return Task.CompletedTask;
        }
        protected override Task DeleteFileCoreAsync(string filePath) { File.Delete(filePath); return Task.CompletedTask; }
        protected override async Task UploadFileFromCoreAsync(string filePath, Stream sourceStream, bool canOverwrite) //will overwrite, probably don't need flag in core method. maybe for rare edge cases 
        {
            using var fs = new FileStream(filePath, FileMode.Create);
            await sourceStream.CopyToAsync(fs).ConfigureAwait(false);
        }
        protected override async Task DownloadFileToCoreAsync(string filePath, Stream destinationStream)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            await fs.CopyToAsync(destinationStream).ConfigureAwait(false);
        }
        protected override Stream CreateReadFileStreamCore(string filePath) => new FileStream(filePath, FileMode.Open, FileAccess.Read);
        protected override Stream CreateWriteFileStreamCore(string filePath, FileMode fileMode) => new FileStream(filePath, fileMode, FileAccess.Write);
        protected override Task<bool> DirectoryExistsCoreAsync(string directoryPath) => Task.FromResult(Directory.Exists(directoryPath));
        protected override Task<string[]> GetDirectoriesCoreAsync(string directoryPath = "/", string searchPattern = "")
        {
            var paths = Directory.GetDirectories(directoryPath, searchPattern);
            this.ReplaceNativeDirectorySeparatorInPaths(paths);
            return Task.FromResult(paths);
        }
        protected override Task CreateDirectoryCoreAsync(string directoryPath) => Task.FromResult(Directory.CreateDirectory(directoryPath));
        protected override Task DeleteDirectoryCoreAsync(string directoryPath) { Directory.Delete(directoryPath, true); return Task.CompletedTask; }


        private void ReplaceNativeDirectorySeparatorInPaths(string[] paths) { for (int i = 0; i < paths.Length; i++) paths[i] = paths[i].Replace(NativeDirectorySeparator, this.DirectorySeparator); }

        public interface IConfiguration : FileIOManager.IConfiguration { }
        public class Configuration : FileIOManager.Configuration, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }
        }
    }

    public interface ILocalDriveFileIOManager : IFileIOManager { }
}

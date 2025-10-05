using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Util;
using Microsoft.Extensions.Configuration;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;

namespace MyNotifier.FileIOManager
{
    //handles errors
    //wraps results in callresult for caller error management 
    public abstract class FileIOManager : IFileIOManager
    {
        protected readonly IConfiguration configuration;
        protected readonly ICallContext<FileIOManager> callContext;

        protected bool isInitialized = false;
        
        protected const string FileNotFoundErrorMessageFormat = "File [{0}] does not exist";
        protected const string FileCannotBeOverwrittenMessageFormat = "File [{0}] already exists and overwrite disallowed";
        protected const string DirectoryNotFoundErrorMessageFormat = "File [{0}] does not exist";
        protected const string DirectoryCannotBeOverwrittenMessageFormat = "Directory [{0}] already exists and overwrite disallowed";

        protected virtual char DirectorySeparator => '/';
        protected virtual string RootDirectoryName => "";

        public FileIOManager(IConfiguration configuration, ICallContext<FileIOManager> callContext) { this.configuration = configuration; this.callContext = callContext; }

        public virtual async ValueTask<ICallResult> InitializeAsync(bool forceReInitialize = false)
        {
            try
            {
                if (this.isInitialized && !forceReInitialize) return new CallResult();

                await this.InitializeCoreAsync().ConfigureAwait(false);

                this.isInitialized = true;

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual async Task<ICallResult<bool>> FileExistsAsync(string filePath)
        {
            try
            {
                var fileExists = await this.FileExistsCoreAsync(filePath).ConfigureAwait(false);
                return new CallResult<bool>(fileExists);
            }
            catch (Exception ex) { return CallResult<bool>.FromException(ex); }
        }
        public virtual async Task<ICallResult<string[]>> GetFilesAsync(string directoryPath = "/", string searchPattern = "") 
        {
            try
            {
                var directoryExists = await this.DirectoryExistsCoreAsync(directoryPath).ConfigureAwait(false);

                if (!directoryExists) return new CallResult<string[]>(false, string.Format(DirectoryNotFoundErrorMessageFormat, directoryPath));

                var files = await this.GetFilesCoreAsync(directoryPath, searchPattern).ConfigureAwait(false);

                return new CallResult<string[]>(files);
            }
            catch (Exception ex) { return CallResult<string[]>.FromException(ex); }
        }
        public virtual async Task<ICallResult> CreateFileAsync(string filePath, bool overwriteExisting = false)
        {
            try
            {
                if (!overwriteExisting &&
                    (await this.FileExistsCoreAsync(filePath).ConfigureAwait(false)))
                    return new CallResult<Stream>(false, BuildFileCannotBeOverwrittenMessage(filePath));

                await this.CreateFileCoreAsync(filePath).ConfigureAwait(false);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }
        public virtual async Task<ICallResult> DeleteFileAsync(string filePath)
        {
            try
            {
                if (!(await this.FileExistsCoreAsync(filePath).ConfigureAwait(false))) return new CallResult(false, BuildFileNotFoundMessage(filePath));

                await this.DeleteFileCoreAsync(filePath).ConfigureAwait(false);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        //probably want to validate state of source/destination streams 
        public virtual async Task<ICallResult> UploadFileFromAsync(string filePath, Stream sourceStream, bool canOverwrite = false)
        {
            try
            {
                if (!canOverwrite && (await this.FileExistsCoreAsync(filePath).ConfigureAwait(false))) return new CallResult(false, $"File: {filePath} exists and cannot be overwritten");

                await this.UploadFileFromCoreAsync(filePath, sourceStream, canOverwrite).ConfigureAwait(false);

                return new CallResult();

            } catch(Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual async Task<ICallResult> DownloadFileToAsync(string filePath, Stream destinationStream)
        {
            try
            {
                await this.DownloadFileToCoreAsync(filePath, destinationStream).ConfigureAwait(false);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual ICallResult<Stream> CreateReadFileStream(string filePath)
        {
            try
            {
                var fileExists = this.FileExistsCoreAsync(filePath).GetAwaiter().GetResult();

                if (!fileExists)
                    return new CallResult<Stream>(false, BuildFileNotFoundMessage(filePath));

                var resultStream = CreateReadFileStreamCore(filePath);

                return new CallResult<Stream>(resultStream);
            }
            catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
        }
        public virtual ICallResult<Stream> CreateWriteFileStream(string filePath, FileMode fileMode, bool overwriteExisting = false)
        {
            try
            {
                var fileExists = this.FileExistsCoreAsync(filePath).GetAwaiter().GetResult();

                if (!overwriteExisting &&
                    fileExists &&
                    fileMode == FileMode.Create)
                    return new CallResult<Stream>(false, BuildFileCannotBeOverwrittenMessage(filePath));

                var resultStream = CreateWriteFileStreamCore(filePath, fileMode);

                return new CallResult<Stream>(resultStream);
            }
            catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
        }
        public virtual async Task<ICallResult<bool>> DirectoryExistsAsync(string directoryPath)
        {
            try
            {
                var directoryExists = await this.DirectoryExistsCoreAsync(directoryPath).ConfigureAwait(false);
                return new CallResult<bool>(directoryExists);
            }
            catch (Exception ex) { return CallResult<bool>.FromException(ex); }
        }
        public virtual async Task<ICallResult<string[]>> GetDirectoriesAsync(string directoryPath = "/", string searchPattern = "")
        {
            try
            {
                var directoryExists = await this.DirectoryExistsCoreAsync(directoryPath).ConfigureAwait(false);

                if (!directoryExists) return new CallResult<string[]>(false, string.Format(DirectoryNotFoundErrorMessageFormat, directoryPath));

                var directories = await this.GetDirectoriesCoreAsync(directoryPath, searchPattern).ConfigureAwait(false);

                return new CallResult<string[]>(directories);
            }
            catch(Exception ex) { return CallResult<string[]>.FromException(ex); }
        }
        public virtual async Task<ICallResult> CreateDirectoryAsync(string directoryPath, bool overwriteExisting = false)
        {
            try
            {
                if ((await this.DirectoryExistsCoreAsync(directoryPath).ConfigureAwait(false)))
                {
                    if (!overwriteExisting) return new CallResult(false, BuildDirectoryCannotBeOverwrittenMessage(directoryPath));

                    await this.DeleteDirectoryCoreAsync(directoryPath).ConfigureAwait(false);
                }

                await this.CreateDirectoryCoreAsync(directoryPath).ConfigureAwait(false);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
        }
        public virtual async Task<ICallResult> DeleteDirectoryAsync(string directoryPath)
        {
            try
            {
                if(!(await this.DirectoryExistsCoreAsync(directoryPath).ConfigureAwait(false))) return new CallResult<string[]>(false, string.Format(DirectoryNotFoundErrorMessageFormat, directoryPath));

                await this.DeleteDirectoryCoreAsync(directoryPath).ConfigureAwait(false);

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected virtual string GetRootNameFromPath(string path) => path.Split(this.DirectorySeparator)[0];
        protected virtual string GetLeafNameFromPath(string path) => path.Split(this.DirectorySeparator)[^1];
        protected virtual string BuildAppendedPath(string root, params string[] paths)                 
        {
            var ret = new List<string>();

            var rootParts = root.Split(this.DirectorySeparator);
            AddTo(ret, rootParts);

            foreach(var p in paths)
            {
                var parts = p.Split(this.DirectorySeparator);
                AddTo(ret, parts);
            }

            return string.Join(this.DirectorySeparator, ret);
        }

        private static void AddTo(IList<string> pathList, string[] parts) { foreach (var p in parts) if (!string.IsNullOrEmpty(p)) pathList.Add(p); }


        protected void GetLeafNameAndRootPathForPath(string path, out string leafName, out string rootPath)
        {
            var parts = path.Split('/');
            leafName = parts[^1];

            if (parts.Length == 0) throw new Exception("Invalid path");
            if (parts.Length == 1) { rootPath = "/"; return; }

            rootPath = string.Join('/', new ArraySegment<string>(parts, 0, parts.Length - 1));
        }

        protected virtual ValueTask InitializeCoreAsync() => new();

        protected abstract Task<bool> FileExistsCoreAsync(string filePath);
        protected abstract Task<string[]> GetFilesCoreAsync(string directoryPath = "/", string searchPattern = "");
        protected abstract Task CreateFileCoreAsync(string filePath);
        protected abstract Task DeleteFileCoreAsync(string filePath);

        protected abstract Task UploadFileFromCoreAsync(string filePath, Stream sourceStream, bool canOverwrite = false);
        protected abstract Task DownloadFileToCoreAsync(string filePath, Stream destinationStream);

        protected abstract Stream CreateReadFileStreamCore(string filePath);
        protected abstract Stream CreateWriteFileStreamCore(string filePath, FileMode fileMode);

        protected abstract Task<bool> DirectoryExistsCoreAsync(string directoryPath);
        protected abstract Task<string[]> GetDirectoriesCoreAsync(string directoryPath = "/", string searchPattern = "");
        protected abstract Task CreateDirectoryCoreAsync(string directoryPath);
        protected abstract Task DeleteDirectoryCoreAsync(string directoryPath);

        protected virtual string BuildFileCannotBeOverwrittenMessage(string filePath) => string.Format(FileCannotBeOverwrittenMessageFormat, filePath);
        protected virtual string BuildFileNotFoundMessage(string filePath) => string.Format(FileNotFoundErrorMessageFormat, filePath);
        protected virtual string BuildDirectoryCannotBeOverwrittenMessage(string directoryPath) => string.Format(DirectoryCannotBeOverwrittenMessageFormat, directoryPath);

        public interface IConfiguration : IConfigurationWrapper { }
        public class Configuration : ConfigurationWrapper, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration) { }

        }

        public class Wrapper : IFileIOManager.IWrapper 
        {
            private readonly IFileIOManager fileIOManager;

            public Wrapper(IFileIOManager fileIOManager) { this.fileIOManager = fileIOManager; }

            public string RootDirectoryName => ((FileIOManager)this.fileIOManager).RootDirectoryName;
            public char DirectorySeparator => ((FileIOManager)this.fileIOManager).DirectorySeparator;

            public string GetLeafFromPath(string path) => ((FileIOManager)this.fileIOManager).GetLeafNameFromPath(path);
            public string GetRootFromPath(string path) => ((FileIOManager)this.fileIOManager).GetRootNameFromPath(path);
            public string BuildAppendedPath(string rootPath, string path) => ((FileIOManager)this.fileIOManager).BuildAppendedPath(rootPath, path);
        }
    }
}

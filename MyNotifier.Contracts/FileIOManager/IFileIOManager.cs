using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.FileIOManager
{
    public interface IFileIOManager
    {

        ValueTask<ICallResult> InitializeAsync(bool forceReInitialize = false);

        //add regex to getfiles and get directories 
        Task<ICallResult<bool>> FileExistsAsync(string filePath);
        Task<ICallResult<string[]>> GetFilesAsync(string directoryPath = "/", string searchPattern = "");
        Task<ICallResult> CreateFileAsync(string filePath, bool overwriteExisting = false);
        Task<ICallResult> DeleteFileAsync(string filePath);
        Task<ICallResult> UploadFileFromAsync(string filePath, Stream sourceStream, bool canOverwrite = false);
        Task<ICallResult> DownloadFileToAsync(string filePath, Stream destinationStream);
        ICallResult<Stream> CreateReadFileStream(string filePath);
        ICallResult<Stream> CreateWriteFileStream(string filePath, FileMode fileMode, bool overwriteExisting = false);
        Task<ICallResult<bool>> DirectoryExistsAsync(string directoryPath);
        Task<ICallResult<string[]>> GetDirectoriesAsync(string directoryPath = "/", string searchPattern = "");
        Task<ICallResult> CreateDirectoryAsync(string directoryPath, bool overwriteExisting = false);
        Task<ICallResult> DeleteDirectoryAsync(string directoryPath);


        //Task<ICallResult> CopyFileAsync(string filePath from, string filePath to);



        //ICallResult<bool> TryCreateFile(string filepath);
        //ICallResult<bool> TryDeleteFile(string filepath);
        //ICallResult<bool> TryCreateDirectory(string directorypath);
        //ICallResult<bool> TryDeleteDirectory(string directorypath)

        public interface IWrapper
        {
            string RootDirectoryName { get; }
            char DirectorySeparator { get; }

            string GetRootFromPath(string path);
            string GetLeafFromPath(string path);
            string BuildAppendedPath(string rootPath, params string[] paths);
        }

    }
}

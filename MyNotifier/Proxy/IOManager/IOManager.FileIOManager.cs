using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Proxy
{
    ////IOManager to implement IFileIOManager to allow Proxy/NotifierPublisher and Proxy/Notifier to derive from FileNotifierPublisher and FileNotifier, respectively, maintining maximal abstraction 
    //public abstract partial class IOManager : IIOManager
    //{
    //    public ValueTask<ICallResult> InitializeAsync(bool forceReInitialize = false) => throw new NotImplementedException(); // ?

    //    public async Task<ICallResult<bool>> FileExistsAsync(string filePath) => await this.fileIOManager.FileExistsAsync(filePath).ConfigureAwait(false);
    //    public async Task<ICallResult<bool>> DirectoryExistsAsync(string directoryPath) => await this.fileIOManager.DirectoryExistsAsync(directoryPath).ConfigureAwait(false);
    //    public async Task<ICallResult> CreateFileAsync(string filePath, bool overwriteExisting = false) => await this.fileIOManager.CreateFileAsync(filePath, overwriteExisting).ConfigureAwait(false);
    //    public async Task<ICallResult> DeleteFileAsync(string filePath) => await this.fileIOManager.DeleteFileAsync(filePath).ConfigureAwait(false);
    //    public async Task<ICallResult> CreateDirectoryAsync(string directoryPath, bool overwriteExisting = false) => await this.fileIOManager.CreateDirectoryAsync(directoryPath, overwriteExisting).ConfigureAwait(false);
    //    public async Task<ICallResult> DeleteDirectoryAsync(string directoryPath) => await this.fileIOManager.DeleteDirectoryAsync(directoryPath).ConfigureAwait(false);
    //    public async Task<ICallResult<string[]>> GetFilesAsync(string directoryPath = "/", string searchPattern = "") => await this.fileIOManager.GetFilesAsync(directoryPath, searchPattern).ConfigureAwait(false);
    //    public async Task<ICallResult<string[]>> GetDirectoriesAsync(string directoryPath = "/", string searchPattern = "") => await this.fileIOManager.GetDirectoriesAsync(directoryPath, searchPattern).ConfigureAwait(false);
    //    public ICallResult<Stream> CreateReadFileStream(string filePath) => this.fileIOManager.CreateReadFileStream(filePath);
    //    public ICallResult<Stream> CreateWriteFileStream(string filePath, FileMode fileMode, bool overwriteExisting = false) => this.fileIOManager.CreateWriteFileStream(filePath, fileMode, overwriteExisting);
    //    public async Task<ICallResult> UploadFileFromAsync(string filePath, Stream sourceStream, bool canOverwrite = false) => await this.fileIOManager.UploadFileFromAsync(filePath, sourceStream, canOverwrite).ConfigureAwait(false);
    //    public async Task<ICallResult> DownloadFileToAsync(string filePath, Stream destinationStream) => await this.fileIOManager.DownloadFileToAsync(filePath, destinationStream).ConfigureAwait(false);
    //}
}

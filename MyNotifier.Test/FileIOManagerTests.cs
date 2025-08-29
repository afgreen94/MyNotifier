using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileIO = MyNotifier.FileIOManager;

namespace MyNotifier.Test
{
    public class LocalDriveFileIOManagerTests : TestBase<LocalDriveFileIOManager, LocalDriveFileIOManager.IConfiguration, LocalDriveFileIOManager.Configuration> { protected override bool skip => false; }
    public class GoogleDriveFileIOManagerTests : TestBase<GoogleDriveFileIOManager, GoogleDriveFileIOManager.IConfiguration, GoogleDriveFileIOManager.Configuration> { protected override bool skip => false; protected override string skipReason => "External Integration."; }


    #region Core

    public abstract class TestBase<TFileIOManagerImplementationType, TFileIOManagerConfigurationServiceType, TFileIOManagerConfigurationImplementationType> : TestClass
        where TFileIOManagerImplementationType : FileIO.FileIOManager, IFileIOManager
        where TFileIOManagerConfigurationServiceType : class, FileIO.FileIOManager.IConfiguration
        where TFileIOManagerConfigurationImplementationType : class, TFileIOManagerConfigurationServiceType
    {

        protected virtual bool skip => false;
        protected virtual string skipReason => "";


        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<TFileIOManagerConfigurationServiceType, TFileIOManagerConfigurationImplementationType>();
            services.AddScoped<IFileIOManager, TFileIOManagerImplementationType>();
            services.AddScoped<ICallContext<TFileIOManagerImplementationType>, CallContext<TFileIOManagerImplementationType>>();
        }

        protected override void InitializeCore() => Assert.False(this.skip, this.skipReason);


        private const string DefaultTestDirectoryPath = "Tests"; //add system root to configuration 
        private const string DefaultTestFilePath = "Tests/TestFile.txt";
        private const int DefaultWriteStreamDelayMs = 2000;

        [Fact]
        public async Task Battery0()
        {
            this.Initialize();

            using var scope = serviceProvider.CreateScope();

            var fileIOManager = scope.ServiceProvider.GetRequiredService<IFileIOManager>();

            var initializeResult = await fileIOManager.InitializeAsync();
            Assert.True(initializeResult.Success, initializeResult.ErrorText);

            //File exists
            var fileExistsResultTrue = await fileIOManager.FileExistsAsync(DefaultTestFilePath);
            Assert.True(fileExistsResultTrue.Success, fileExistsResultTrue.ErrorText); Assert.True(fileExistsResultTrue.Result, fileExistsResultTrue.ErrorText);

            var fileExistsResultFalse = await fileIOManager.FileExistsAsync(DefaultTestDirectoryPath + $"/non-existentFile{Guid.NewGuid()}");
            Assert.True(fileExistsResultFalse.Success, fileExistsResultFalse.ErrorText); Assert.False(fileExistsResultFalse.Result, fileExistsResultFalse.ErrorText);

            //DirectoryExists
            var directoryExistsResultTrue = await fileIOManager.DirectoryExistsAsync(DefaultTestDirectoryPath);
            Assert.True(directoryExistsResultTrue.Success, directoryExistsResultTrue.ErrorText); Assert.True(directoryExistsResultTrue.Result, directoryExistsResultTrue.ErrorText);

            var directoryExistsResultFalse = await fileIOManager.DirectoryExistsAsync(DefaultTestDirectoryPath + $"/RandomDirectoryName{Guid.NewGuid()}");
            Assert.True(directoryExistsResultFalse.Success, directoryExistsResultFalse.ErrorText); Assert.False(directoryExistsResultFalse.Result, directoryExistsResultFalse.ErrorText);


            BuildSessionTestArgs(out var sessionTestDirectory, out var sessionTestFile, out var sessionGuid);


            //CreateDirectory
            var createDirectoryResult = await fileIOManager.CreateDirectoryAsync(sessionTestDirectory);
            Assert.True(createDirectoryResult.Success, createDirectoryResult.ErrorText);

            //Create File Test 
            var createAndDeleteFileTestFile = string.Format(sessionTestFile, "CreateFile");

            var createFileResult = await fileIOManager.CreateFileAsync(createAndDeleteFileTestFile, true);
            Assert.True(createFileResult.Success, createFileResult.ErrorText);

            var fileExistsResult1 = await fileIOManager.FileExistsAsync(createAndDeleteFileTestFile);
            Assert.True(fileExistsResult1.Success, fileExistsResult1.ErrorText); Assert.True(fileExistsResult1.Result, fileExistsResult1.ErrorText);

            //GetFiles
            var getFilesResult = await fileIOManager.GetFilesAsync(sessionTestDirectory);
            Assert.True(getFilesResult.Success, getFilesResult.ErrorText); Assert.NotEmpty(getFilesResult.Result);

            //GetDirectories 
            var getdirectoriesResult = await fileIOManager.GetDirectoriesAsync(DefaultTestDirectoryPath);
            Assert.True(getdirectoriesResult.Success, getdirectoriesResult.ErrorText); Assert.NotEmpty(getdirectoriesResult.Result);

            //Stream Tests
            var createStreamTestFile = string.Format(sessionTestFile, "CreateStream");

            var createWriteStreamResult = fileIOManager.CreateWriteFileStream(createStreamTestFile, FileMode.Create, true);
            Assert.True(createWriteStreamResult.Success, createWriteStreamResult.ErrorText);


            using (var sw = new StreamWriter(createWriteStreamResult.Result))
                await sw.WriteAsync("Hello World");

            await Task.Delay(DefaultWriteStreamDelayMs);

            var createReadStreamResult = fileIOManager.CreateReadFileStream(createStreamTestFile);
            Assert.True(createReadStreamResult.Success, createReadStreamResult.ErrorText);

            string actual = "";
            using (var sr = new StreamReader(createReadStreamResult.Result))
                actual = await sr.ReadToEndAsync();

            Assert.Equal("Hello World", actual);

            //DeleteFile
            var deleteFileResult = await fileIOManager.DeleteFileAsync(createAndDeleteFileTestFile);
            Assert.True(deleteFileResult.Success, deleteFileResult.ErrorText);

            var fileExistsResult2 = await fileIOManager.FileExistsAsync(createAndDeleteFileTestFile);
            Assert.True(fileExistsResult2.Success, fileExistsResult2.ErrorText); Assert.False(fileExistsResult2.Result, fileExistsResult2.ErrorText);

            //DeleteDirectory
            var deleteDirectoryResult = await fileIOManager.DeleteDirectoryAsync(sessionTestDirectory);
            Assert.True(deleteDirectoryResult.Success, deleteDirectoryResult.ErrorText);

            var directoryExistsResult1 = await fileIOManager.DirectoryExistsAsync(sessionTestDirectory);
            Assert.True(directoryExistsResult1.Success, directoryExistsResult1.ErrorText); Assert.False(directoryExistsResult1.Result, directoryExistsResult1.ErrorText);


            await ClearTestEnvironmentAsync(fileIOManager);
        }




        protected static void BuildSessionTestArgs(out string testDirectoryName, out string testFilename, out Guid sessionGuid)
        {
            sessionGuid = Guid.NewGuid();

            testDirectoryName = DefaultTestDirectoryPath + $"/TestDirectory_{sessionGuid}";
            testFilename = testDirectoryName + $"/TestFile_{{0}}_{sessionGuid}.txt";
        }

        protected static async Task ClearTestEnvironmentAsync(IFileIOManager fileIOManager)
        {
            var directories = (await fileIOManager.GetDirectoriesAsync(DefaultTestDirectoryPath)).Result;
            foreach (var d in directories) await fileIOManager.DeleteDirectoryAsync(d);
        }
    }


    internal class InMemoryFileIOManager : FileIO.FileIOManager, IInMemoryFileIOManager
    {
        private readonly DirectoryObject mockFileSystem = new();

        internal InMemoryFileIOManager(IConfiguration configuration, ICallContext<FileIO.FileIOManager> callContext) : base(configuration, callContext) { }

        protected override Task CreateDirectoryCoreAsync(string directoryPath)
        {
            throw new NotImplementedException();
        }

        protected override Task CreateFileCoreAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Stream CreateReadFileStreamCore(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Stream CreateWriteFileStreamCore(string filePath, FileMode fileMode)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteDirectoryCoreAsync(string directoryPath)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteFileCoreAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DirectoryExistsCoreAsync(string directoryPath)
        {
            throw new NotImplementedException();
        }

        protected override Task DownloadFileToCoreAsync(string filePath, Stream destinationStream)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> FileExistsCoreAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        protected override Task<string[]> GetDirectoriesCoreAsync(string directoryPath = "/", string searchPattern = "")
        {
            throw new NotImplementedException();
        }

        protected override Task<string[]> GetFilesCoreAsync(string directoryPath = "/", string searchPattern = "")
        {
            throw new NotImplementedException();
        }

        protected override Task UploadFileFromCoreAsync(string filePath, Stream sourceStream, bool canOverwrite = false)
        {
            throw new NotImplementedException();
        }


        internal interface IConfiguration : FileIO.FileIOManager.IConfiguration { }

        internal class Configuration : FileIO.FileIOManager.Configuration, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }
        }

        class Object
        {
            public string Name { get; set; }
        }

        class DirectoryObject : Object
        {
            public FileObject[] Files { get; set; }
            public DirectoryObject[] Directories { get; set; }
        }

        class FileObject : Object
        {
            public byte[] Data;
        }
    }

    interface IInMemoryFileIOManager : IFileIOManager { }

    #endregion Core
}

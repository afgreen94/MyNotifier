using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.CommandAndControl;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using MyNotifier.FileIOManager;
using MyNotifier.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Test
{
    public class ProxyFileServerInitializerTest : TestClass
    {
        protected override string AppsettingsPath => "appsettings.proxyFileServerInitializer.test.json";


        protected SystemSettings systemSettings = new()
        {
            Scheme = SystemScheme.ProxyFileIOServer,
            Settings = new ProxySettings()
            {
                AllowRecreateProxyFileSystem = true,
                ProxyHost = FileStorageProvider.Local
            }
        };

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IApplicationConfiguration>(new ApplicationConfiguration(this.systemSettings));

            services.AddScoped<ICallContext<ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>, CallContext<ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>>();
            services.AddScoped<ProxyFileServerInitializer.IProxyFileServerInitializerIOManager, ProxyFileServerInitializer.ProxyFileServerInitializerIOManager>();

            services.AddScoped<ProxyFileServerInitializer.IConfiguration, ProxyFileServerInitializer.Configuration>();
            services.AddScoped<ICallContext<ProxyFileServerInitializer>, CallContext<ProxyFileServerInitializer>>();
            services.AddScoped<IProxyFileServerInitializer, ProxyFileServerInitializer>();
        }


        [Fact(Skip = "External Integration")] //external integration using google drive 
        public async Task PFSITest0_GoogleDrive_Async()  
        {
            this.registerTestServices = (services) =>
            {
                services.AddScoped<GoogleDriveFileIOManager.IConfiguration, GoogleDriveFileIOManager.Configuration>();
                services.AddScoped<ICallContext<GoogleDriveFileIOManager>, CallContext<GoogleDriveFileIOManager>>();
                services.AddScoped<IFileIOManager, GoogleDriveFileIOManager>();
            };

            await this.PFSITest0_Core_Async();
        }

        [Fact]  //internal integration using local drive
        public async Task PSITest0_LocalDrive_Async()
        {
            this.registerTestServices = (services) =>
            {
                services.AddScoped<LocalDriveFileIOManager.IConfiguration, LocalDriveFileIOManager.Configuration>();
                services.AddScoped<ICallContext<LocalDriveFileIOManager>, CallContext<LocalDriveFileIOManager>>();
                services.AddScoped<IFileIOManager, LocalDriveFileIOManager>();
            };

            await this.PFSITest0_Core_Async();
        }

        private async Task PFSITest0_Core_Async()
        {
            this.Initialize();

            await this.CreateTestEnvironmentAsync();

            var initializeResult = await this.serviceProvider.GetRequiredService<IProxyFileServerInitializer>().InitializeSystemAsync();

            Assert.True(initializeResult.Success, initializeResult.ErrorText);

            await this.ClearTestEnvironmentAsync();
        }


        private int writeTasksDelayMs = 1000;
        private async Task CreateTestEnvironmentAsync()
        {
            var fileIOManager = this.serviceProvider.GetRequiredService<IFileIOManager>();

            var rootExists = await fileIOManager.DirectoryExistsAsync("Root"); Assert.True(rootExists.Success);

            if (rootExists.Result)
            {
                var deleteRootResult = await fileIOManager.DeleteDirectoryAsync("Root");
                Assert.True(deleteRootResult.Success);
            }

            string[] directories =
            [
                "Root",
                "Root/Updaters",
                "Root/Updaters/Dlls",
                "Root/Notifications",
                "Root/Commands",
                "Root/Exceptions"
            ];

            foreach (var d in directories) Assert.True((await fileIOManager.CreateDirectoryAsync(d)).Success);

            string[][] files =
            [
                [ "Root/Catalog", "{}" ],
                [ "Root/Interests", "[]" ]
            ];

            foreach (var f in files)
            {
                var createWriteStreamResult = fileIOManager.CreateWriteFileStream(f[0], FileMode.Create, true); Assert.True(createWriteStreamResult.Success, createWriteStreamResult.ErrorText);

                using var sw = new StreamWriter(createWriteStreamResult.Result);
                await sw.WriteAsync(f[1]);
            }


            await Task.Delay(this.writeTasksDelayMs);

            //need test updater dlls located in root/updaters/dlls 
            //will have to have dummy dll and load into folder


        }

        private async Task ClearTestEnvironmentAsync()
        {
            var fileIOManager = this.serviceProvider.GetRequiredService<IFileIOManager>();

            var deleteResult = await fileIOManager.DeleteDirectoryAsync("Root");

            Assert.True(deleteResult.Success, deleteResult.ErrorText);
        }

        protected class ApplicationConfiguration : IApplicationConfiguration
        {
            private readonly SystemSettings systemSettings;

            public SystemSettings SystemSettings => this.systemSettings;
            public DriverLoopSettings DriverLoopSettings => null;
            public IConfiguration InnerConfiguration => null;

            public IControllable Controllable => throw new NotImplementedException();

            public ApplicationConfiguration(SystemSettings systemSettings) => this.systemSettings = systemSettings;
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.FileIOManager;
using MyNotifier.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Notifiers;
using MyNotifier.Contracts.Notifiers;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;

namespace MyNotifier.Test
{
    public class FileSystemNotifierPublisherTests : TestClass 
    {

        public static string PublishDirectoryRoot = "Root/Notifications";
        public static string DataFileName = "Data";
        public static string MetadataFileName = "Metadata";

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<INotificationFileSystemObjectTranslator, DefaultTranslator>();

            this.RegisterNotifierPublisherServices(services);
            this.RegisterNotifierServices(services);
        }


        private void RegisterNotifierPublisherServices(IServiceCollection services)
        {
            services.AddSingleton<FileNotifierPublisher.IConfiguration>(new NotifierPublisherConfiguration());

            services.AddScoped<ICallContext<FileNotifierPublisher>, CallContext<FileNotifierPublisher>>();
            services.AddScoped<IFileNotifierPublisher, FileNotifierPublisher>();

        }

        private void RegisterNotifierServices(IServiceCollection services)
        {
            services.AddSingleton<FileNotifier.IConfiguration>(new NotifierConfiguration());

            services.AddScoped<ICallContext<FileNotifier>, CallContext<FileNotifier>>();
            services.AddScoped<IFileNotifier, FileNotifier>();
        }


        public class NotifierPublisherConfiguration : FileNotifierPublisher.IConfiguration
        {
            private readonly FileNotifierPublisher.WriteCompleteSignalArgs wcsArgs = new();
            public IConfiguration InnerConfiguration => null;
            public string PublishDirectoryRoot => FileSystemNotifierPublisherTests.PublishDirectoryRoot;
            public bool AllowOverwriteExistingNotification => true;
            public string DefaultMetadataFileName => MetadataFileName;
            public string DefaultDataFileName => DataFileName;
            public FileNotifierPublisher.WriteCompleteSignalArgs WriteCompleteSignalArgs => this.wcsArgs;
        }

        public class NotifierConfiguration : FileNotifier.IConfiguration
        {
            private readonly FileNotifier.WriteCompleteSignalArgs wcsArgs = new();
            private readonly FileNotifier.AllowedNotificationTypeArgs allowedNotificationArgs = new() { Updates = true, Commands = true, Exceptions = true };
            private readonly TimeSpan clearCacheInterval = new(0, 30, 0);

            public IConfiguration InnerConfiguration => null;
            public string NotificationsDirectoryName => PublishDirectoryRoot;
            public string MetadataFileName => MetadataFileName;
            public string DataFileName => DataFileName;
            public FileNotifier.AllowedNotificationTypeArgs AllowedNotificationTypeArgs => this.allowedNotificationArgs;
            public int DisconnectionAttemptsCount => 3;
            public int TryDisconnectLoopDelayMs => 5000;
            public int NotificationPollingDelayMs => 1000;
            public bool DeleteNotificationOnDelivered => true;
            public TimeSpan ClearCacheInterval => this.clearCacheInterval;
            public FileNotifier.WriteCompleteSignalArgs WriteCompleteSignalArgs => this.wcsArgs;
        }


        [Fact(Skip = "Not Implemented")]
        public async Task FileNotifierPublisher_InMemory_TestsAsync()
        {
            this.SetInMemoryFileSystemRegistrarDelegate();
            await this.PublisherTestsCoreAsync();
        }

        [Fact]
        public async Task FileNotifierPublisher_LocalDrive_TestsAsync()
        {
            this.SetLocalDriveFileSystemRegistrarDelegate();
            await this.PublisherTestsCoreAsync();
        }

        [Fact(Skip = "External Integration")]
        public async Task FileNotifierPublisher_GoogleDrive_TestsAsync()
        {
            this.SetGoogleDriveRegistrarDelegate();
            await this.PublisherTestsCoreAsync();
        }

        [Fact(Skip = "Not Implemented")]
        public async Task FileNotifier_InMemory_TestsAsync()
        {
            this.SetInMemoryFileSystemRegistrarDelegate();
            await this.NotifierTestsCoreAsync();
        }

        [Fact]
        public async Task FileNotifier_LocalDrive_TestsAsync()
        {
            this.SetLocalDriveFileSystemRegistrarDelegate();
            await this.NotifierTestsCoreAsync();
        }

        [Fact(Skip = "External Integration")]
        public async Task FileNotifier_GoogleDrive_TestsAsync()
        {
            this.SetGoogleDriveRegistrarDelegate();
            await this.NotifierTestsCoreAsync();
        }

        [Fact]
        public async Task FullSystem_LocalDrive_TestsAsync()
        {
            this.SetLocalDriveFileSystemRegistrarDelegate();
            await this.IntegrationTestCoreAsync();
        }

        [Fact(Skip = "External Integration")]
        public async Task FullSystem_GoogleDrive_TestsAsync()
        {
            this.SetGoogleDriveRegistrarDelegate();
            await this.IntegrationTestCoreAsync();
        }

        //[Fact]
        //public void DefaultTranslatorTests()
        //{
        //    using var scope = this.serviceProvider.CreateScope();
        //    var translator = scope.ServiceProvider.GetRequiredService<INotificationFileSystemObjectTranslator>();

        //    var inputFolderObjectDescription = new NotificationFolderObjectDescription()
        //    {
        //        InterestId = Guid.NewGuid(),
        //        UpdaterId = Guid.NewGuid(),
        //        Ticks = DateTime.UtcNow.Ticks,
        //        Type = NotificationType.Update
        //    };

        //    var folderName = translator.ToFolderName()
        //}

        #region Helpers

        private const int DefaultNotificationReceiptLatencyMs = 2000;
        private const string TestUpdateMessage = "This is an Update.";
        private PublishArgs[] testPublications = 
        [
            new()
            {
                InterestId = Guid.NewGuid(),
                UpdaterId = Guid.NewGuid(),
                UpdateTime = DateTime.UtcNow,
                TypeArgs = new() { NotificationType = NotificationType.Update, NotificationDataTypeArgs = new() { DataType = NotificationDataType.String_Generic, Description = "UTF8" } },
                Data = Encoding.UTF8.GetBytes(TestUpdateMessage)
            },
            new()
            {
                UpdateTime = DateTime.UtcNow,
                TypeArgs = new() { NotificationType = NotificationType.Command, NotificationDataTypeArgs = new() { DataType = NotificationDataType.String_Generic, Description = "UTF8" } },
                Data = Encoding.UTF8.GetBytes(TestUpdateMessage)
            },
            new()
            {
                UpdateTime = DateTime.UtcNow,
                TypeArgs = new() { NotificationType = NotificationType.Exception, NotificationDataTypeArgs = new() { DataType = NotificationDataType.String_Generic, Description = "UTF8" } },
                Data = Encoding.UTF8.GetBytes(TestUpdateMessage)
            }
        ];

        private async Task PublisherTestsCoreAsync()
        {

            this.Initialize();

            using var scope = this.serviceProvider.CreateScope();

            var publisher = scope.ServiceProvider.GetRequiredService<IFileNotifierPublisher>();

            var publisherInitializeResult = await publisher.InitializeAsync();
            Assert.True(publisherInitializeResult.Success, publisherInitializeResult.ErrorText);

            var fileIOManager = scope.ServiceProvider.GetRequiredService<IFileIOManager>();

            var fileIOManagerInitializeResult = await fileIOManager.InitializeAsync();
            Assert.True(fileIOManagerInitializeResult.Success, fileIOManagerInitializeResult.ErrorText);

            await CreateTestEnvironmentAsync(fileIOManager);

            foreach (var pa in this.testPublications) await PublishTextAndValidatePublicationAsync(publisher, fileIOManager, pa);

            await ClearTestEnvironmentAsync(fileIOManager);
        }

        private async Task NotifierTestsCoreAsync()
        {
            this.Initialize();

            using var scope = this.serviceProvider.CreateScope();

            var fileIOManager = scope.ServiceProvider.GetRequiredService<IFileIOManager>();

            var fileIOManagerInitializeResult = await fileIOManager.InitializeAsync();
            Assert.True(fileIOManagerInitializeResult.Success, fileIOManagerInitializeResult.ErrorText);

            await CreateTestEnvironmentAsync(fileIOManager);

            var notifier = scope.ServiceProvider.GetRequiredService<IFileNotifier>();

            //subscribe someone to notifications 
            var subscriber = new NotifierSubscriber();

            notifier.Subscribe(subscriber);

            var connectResult = await notifier.ConnectAsync(null);
            Assert.True(connectResult.Success, connectResult.ErrorText);
            Assert.True(notifier.Connected);

            await Task.Delay(3000);

            notifier.Unsubscribe(subscriber);

            var disconnectResult = await notifier.DisconnectAsync();
            Assert.True(disconnectResult.Success, disconnectResult.ErrorText);

            await Task.Delay(3000);

            Assert.False(notifier.Connected);
        }

        private async Task IntegrationTestCoreAsync()
        {
            this.Initialize();

            using var scope = this.serviceProvider.CreateScope();

            var fileIOManager = scope.ServiceProvider.GetRequiredService<IFileIOManager>();
            var fileIOManagerInitResult = await fileIOManager.InitializeAsync();
            Assert.True(fileIOManagerInitResult.Success, fileIOManagerInitResult.ErrorText);

            await CreateTestEnvironmentAsync(fileIOManager);

            var publisher = scope.ServiceProvider.GetRequiredService<IFileNotifierPublisher>();
            var publisherInitResult = await publisher.InitializeAsync();
            Assert.True(publisherInitResult.Success, publisherInitResult.ErrorText);

            var notifier = scope.ServiceProvider.GetRequiredService<IFileNotifier>();

            var subscriber = new NotifierSubscriber();
            notifier.Subscribe(subscriber);

            var notifierConnectResult = await notifier.ConnectAsync(null);
            Assert.True(notifierConnectResult.Success, notifierConnectResult.ErrorText);

            var publishResult = await publisher.PublishAsync(this.testPublications[0]);
            Assert.True(publishResult.Success, publishResult.ErrorText);

            await Task.Delay(10000);

            Assert.NotEmpty(subscriber.ReceivedNotifications);

            //compare notification to publication 


            await ClearTestEnvironmentAsync(fileIOManager);
        }

        private static async Task PublishTextAndValidatePublicationAsync(IFileNotifierPublisher publisher, 
                                                                         IFileIOManager fileIOManager,
                                                                         PublishArgs publishArgs, 
                                                                         string expectedDataFileText = TestUpdateMessage,
                                                                         bool validate = false)
        {
            var publishResult = await publisher.PublishAsync(publishArgs);
            Assert.True(publishResult.Success, publishResult.ErrorText);


            if (validate)
            {
                var wrapper = new FileIOManager.FileIOManager.Wrapper(fileIOManager);

                string notificationFolderPath = "";
                var notificationFolderExistsResult = await fileIOManager.DirectoryExistsAsync(notificationFolderPath);

                string metadataFilePath = "";
                string expectedMetadataFileText = "";
                await ValidateNotificationFileAsync(fileIOManager, metadataFilePath, expectedMetadataFileText);

                string dataFilePath = "";
                await ValidateNotificationFileAsync(fileIOManager, dataFilePath, expectedDataFileText);

                //validate writeCompleteSignal 
            }
        }

        private static async Task ValidateNotificationFileAsync(IFileIOManager fileIOManager, string filePath, string expectedText)
        {
            var createReadStreamResult = fileIOManager.CreateReadFileStream(filePath);
            Assert.True(createReadStreamResult.Success, createReadStreamResult.ErrorText);

            var actualText = "";
            using (var sr = new StreamReader(createReadStreamResult.Result, Encoding.UTF8))
                actualText = await sr.ReadToEndAsync();

            Assert.Equal(expectedText, actualText);
        }

        private static async Task CreateTestEnvironmentAsync(IFileIOManager fileIOManager)
        {
            var wrapper = new FileIOManager.FileIOManager.Wrapper(fileIOManager);

            var rootFolderName = "Root";

            string[] folders =
            [
                rootFolderName,
                wrapper.BuildAppendedPath(rootFolderName, "Notifications"),
                wrapper.BuildAppendedPath(rootFolderName, "Commands"),
                wrapper.BuildAppendedPath(rootFolderName, "Exceptions")
            ];

            foreach(var f in folders)
            {
                var createResult = await fileIOManager.CreateDirectoryAsync(f, true);
                Assert.True(createResult.Success, createResult.ErrorText);
            }
        }

        private static async Task ClearTestEnvironmentAsync(IFileIOManager fileIOManager)
        {
            var deleteResult = await fileIOManager.DeleteDirectoryAsync("Root");

            Assert.True(deleteResult.Success, deleteResult.ErrorText);
        }

        private void SetInMemoryFileSystemRegistrarDelegate() => this.registerTestServices = (sc) =>
        {
            sc.AddSingleton<InMemoryFileIOManager.IConfiguration, InMemoryFileIOManager.Configuration>();
            sc.AddScoped<ICallContext<InMemoryFileIOManager>, CallContext<InMemoryFileIOManager>>();
            sc.AddScoped<IInMemoryFileIOManager, InMemoryFileIOManager>();
        };

        private void SetLocalDriveFileSystemRegistrarDelegate() => this.registerTestServices = (sc) =>
        {
            sc.AddSingleton<LocalDriveFileIOManager.IConfiguration, LocalDriveFileIOManager.Configuration>();
            sc.AddScoped<ICallContext<LocalDriveFileIOManager>, CallContext<LocalDriveFileIOManager>>();
            sc.AddScoped<IFileIOManager, LocalDriveFileIOManager>();
        };


        private void SetGoogleDriveRegistrarDelegate() => this.registerTestServices = (sc) =>
        {
            sc.AddSingleton<GoogleDriveFileIOManager.IConfiguration, GoogleDriveFileIOManager.Configuration>();
            sc.AddScoped<ICallContext<GoogleDriveFileIOManager>, CallContext<GoogleDriveFileIOManager>>();
            sc.AddScoped<IFileIOManager, GoogleDriveFileIOManager>();
        };

        private class NotifierSubscriber : INotifier.ISubscriber
        {
            private Definition definition = new() { Id = Guid.NewGuid(), Name = "", Description = "" };
            public Definition Definition => this.definition;
            public IList<Notification> ReceivedNotifications = [];
            public async ValueTask OnNotificationAsync(object sender, Notification notification) => this.ReceivedNotifications.Add(notification);
        }

        #endregion Helpers
    }
}

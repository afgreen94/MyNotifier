using MyNotifier.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Proxy;
using MyNotifier.Contracts.Proxy;

namespace MyNotifier.Publishers
{
    public partial class FileNotifierPublisher : NotifierPublisher, IFileNotifierPublisher
    {
        protected const string DefaultWriteException = "Failed to write notification/metadata file.";

        private readonly IFileIOManager fileIOManager;
        private readonly INotificationFileSystemObjectTranslator translator;
        private readonly IConfiguration configuration;
        private readonly ICallContext callContext;

        private readonly NotificationBuilder notificationBuilder;
        private readonly PublisherFileIOHelper fileIOHelper; 

        public FileNotifierPublisher(IFileIOManager fileIOManager,
                                     INotificationFileSystemObjectTranslator translator,
                                     IConfiguration configuration,
                                     ICallContext<FileNotifierPublisher> callContext)
        {
            this.fileIOManager = fileIOManager;
            this.translator = translator;
            this.configuration = configuration;
            this.callContext = callContext;

            this.fileIOHelper = new(this.fileIOManager, this.translator, this.configuration);
            this.notificationBuilder = new(this.configuration);
        }

        protected override async ValueTask<ICallResult> InitializeCoreAsync()
        {
            try
            {
                var fileIOManagerInitializeResult = await this.fileIOManager.InitializeAsync().ConfigureAwait(false);
                if (!fileIOManagerInitializeResult.Success) return CallResult.BuildFailedCallResult(fileIOManagerInitializeResult, "Failed to initialize fileIOManager: {0}");

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected override async ValueTask<ICallResult> PublishCoreAsync(PublishArgs args)
        {
            var notification = this.notificationBuilder.BuildNotification(args);
            return await this.fileIOHelper.WriteNotificationFilesAsync(notification).ConfigureAwait(false);
        }
    }

    public interface IFileNotifierPublisher : INotifierPublisher { }
}

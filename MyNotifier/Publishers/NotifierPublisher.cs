using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Publishers
{
    public abstract class NotifierPublisher : INotifierPublisher
    {
        private const string NotInitializedMessage = "Not initialized.";

        //protected readonly IConfiguration configuration;

        //protected NotifierPublisher(IConfiguration configuration) { this.configuration = configuration; }

        protected bool isInitialized = false;

        public virtual async ValueTask<ICallResult> InitializeAsync(bool forceReinitialize = false)
        {
            try
            {
                if (!this.isInitialized || forceReinitialize)
                {
                    await this.InitializeCoreAsync().ConfigureAwait(false);

                    this.isInitialized = true;
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        public virtual async ValueTask<ICallResult> PublishAsync(Notification notification)
        {
            try
            {
                if (!this.isInitialized) return new CallResult(false, NotInitializedMessage);

                var nowTime = DateTime.UtcNow;

                notification.Metadata.Description.Header.Ticks = nowTime.Ticks;
                notification.Metadata.Description.PublishedAt = nowTime;

                return await this.PublishCoreAsync(notification).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
            throw new NotImplementedException();
        }

        protected virtual ValueTask<ICallResult> InitializeCoreAsync() { return new ValueTask<ICallResult>(new CallResult()); }
        protected abstract ValueTask<ICallResult> PublishCoreAsync(Notification notification);


        public interface IConfiguration : IConfigurationWrapper { }

        public class Configuration : ConfigurationWrapper, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }
        }
    }
}

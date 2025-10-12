using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual async ValueTask<ICallResult> PublishAsync(PublishArgs publishArgs)
        {
            try
            {
                if (!this.isInitialized) return new CallResult(false, NotInitializedMessage);
                return await this.PublishCoreAsync(publishArgs).ConfigureAwait(false);
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }


        protected virtual Notification BuildNotification(PublishArgs args) => throw new NotImplementedException(); //new()
        //{
        //    Metadata = new NotificationMetadata()
        //    {
        //        Definition = new NotificationDefinition() { InterestId = args.InterestId, UpdaterId = args.UpdaterId },
        //        TypeArgs = args.TypeArgs,
        //        UpdatedAt = args.UpdateTime,
        //        SizeBytes = args.Data.Length,
        //        //encrypted?
        //    },
        //    Data = args.Data
        //};

        protected virtual ValueTask<ICallResult> InitializeCoreAsync() { return new ValueTask<ICallResult>(new CallResult()); }
        protected abstract ValueTask<ICallResult> PublishCoreAsync(PublishArgs publishArgs);


        public interface IConfiguration : IConfigurationWrapper { }

        public class Configuration : ConfigurationWrapper, IConfiguration
        {
            public Configuration(Microsoft.Extensions.Configuration.IConfiguration innerConfiguration) : base(innerConfiguration)
            {
            }
        }
    }
}

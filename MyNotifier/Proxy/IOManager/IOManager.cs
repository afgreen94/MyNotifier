using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Notifications;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//using static MyNotifier.Notifiers.FileNotifier;
using EventModuleDefinitionModel = MyNotifier.Contracts.EventModules.DefinitionModel;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;

namespace MyNotifier.Proxy //namespacing??
{
    public abstract partial class IOManager : IIOManager //should not be abstract class ? //allow method calls to reinitialize? //should not be abstract //create wrapper for try/catch + AssertInitialized, have config for allow internal reinit 
    {

        protected const string NotInitializedMessage = "Not Initialized";

        protected readonly IFileIOManager fileIOManager;
        protected readonly IConfiguration configuration;
        protected readonly ICallContext<IOManager> callContext;

        protected IFileIOManager.IWrapper fileIOManagerWrapper;
        protected IProxySettings proxySettings;
        protected Paths paths;

        protected bool isInitialized = false;

        public IProxySettings ProxySettings => this.proxySettings;

        protected IOManager(IFileIOManager fileIOManager, IConfiguration configuration, ICallContext<IOManager> callContext)
        {
            this.fileIOManager = fileIOManager;
            this.configuration = configuration;
            this.callContext = callContext;
        }

        //public virtual async Task<ICallResult> InitializeAsync(bool forceReinitialize) => await this.InitializeCoreAsync(forceReinitialize).ConfigureAwait(false);

        public virtual async Task<ICallResult> InitializeAsync(IProxySettings proxySettings, bool forceReInitialize = false)
        {
            try
            {
                if (!this.isInitialized || forceReInitialize)
                {
                    var validateProxySettingsResult = this.ValidateProxySettings(); if (!validateProxySettingsResult.Success) return validateProxySettingsResult;

                    var fileIOManagerInitResult = await this.fileIOManager.InitializeAsync().ConfigureAwait(false);
                    if (!fileIOManagerInitResult.Success) return CallResult.BuildFailedCallResult(fileIOManagerInitResult, "Failed to initialize fileIOManager");

                    this.paths = proxySettings.FileStructure.BuildPaths(new FileIOManager.FileIOManager.Wrapper(this.fileIOManager));
                    //this.moduleLoader = new ModuleLoader(this.fileIOManager, this.paths);

                    this.isInitialized = true;
                }

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }

        protected async Task<ICallResult<TModel>> RetrieveDefinitionModelAsync<TModel>(string path, Guid id, string semanticNamePrefix)
        {
            try
            {
                var filePath = this.fileIOManagerWrapper.BuildAppendedPath(path, id.ToString());

                var createStreamResult = this.fileIOManager.CreateReadFileStream(filePath);
                if (!createStreamResult.Success) return CallResult<TModel>.BuildFailedCallResult(createStreamResult, $"Failed to create read stream for model file of {semanticNamePrefix}Definition with Id: {id}");

                string definitionJson;

                using (var sr = new StreamReader(createStreamResult.Result)) definitionJson = await sr.ReadToEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(definitionJson)) return new CallResult<TModel>(false, $"Failed to retrieve json for model of {semanticNamePrefix}Definition with Id: {id}: Empty json.");

                var model = JsonSerializer.Deserialize<TModel>(definitionJson);
                if (model == null) return new CallResult<TModel>(false, $"Failed to deserialize json for {semanticNamePrefix}DefinitionModel with Id: {id}: Invalid json.");

                return new CallResult<TModel>(model);

            }
            catch (Exception ex) { return CallResult<TModel>.FromException(ex); }
        }


        protected ICallResult ValidateProxySettings()
        {
            try
            {
                if (this.proxySettings == null) return new CallResult(false, "Proxy settings absent");

                //validation logic, ensure no paths are null or empty
                //ensure fileIOManager type matches proxyHost 

                return new CallResult();
            }
            catch (Exception ex) { return CallResult.FromException(ex); }
        }


        protected const string NotificationIdToken = "ID";
        protected const string UpdateTicksToken = "TICKS";
        protected const string NotificationTypeToken = "TYPE";
        protected const char FormatDelimiter = '_';
        protected readonly string notificationDirectoryNameFormat = $"{NotificationIdToken}{FormatDelimiter}{UpdateTicksToken}{FormatDelimiter}{NotificationTypeToken}";

        protected virtual NotificationHeader ToNotificationHeader(string directoryPath)
        {
            var parts = directoryPath.Split(this.fileIOManagerWrapper.DirectorySeparator)[^1]
                                     .Split(FormatDelimiter);

            return new()
            {
                Id = Guid.Parse(parts[0]),
                Ticks = long.Parse(parts[1]),
                Type = Contracts.EnumStringMaps.GetNotificationType(parts[2])
            };
        }

        protected virtual string ToFolderName(NotificationHeader notificationHeader) => this.notificationDirectoryNameFormat.Replace(NotificationIdToken, notificationHeader.Id.ToString())
                                                                                                                            .Replace(UpdateTicksToken, notificationHeader.Ticks.ToString())
                                                                                                                            .Replace(NotificationTypeToken, Contracts.EnumStringMaps.GetString(notificationHeader.Type));

        protected static ICallResult BuildNotInitializedCallResult() => new CallResult(false, NotInitializedMessage);
        protected static ICallResult<T> BuildNotInitializedCallResult<T>() => new CallResult<T>(false, NotInitializedMessage);

        //protected static ICallResult<T> BuildFailedToReadFileCallResult<T>(string semanticNamePrefix, Guid id, string errorText = "")
        //{
        //    var errorMessage = $"Failed to read file of {semanticNamePrefix}Definition with Id: {id}";

        //    if (!string.IsNullOrEmpty(errorText)) errorMessage = $"{errorMessage}: {errorText}";

        //    return new CallResult<T>(false, errorMessage);
        //}

        //protected static ICallResult<T> BuildFailedToDeserializeJsonCallResult<T>(string semanticNamePrefix, Guid id) => BuildFailedToReadFileCallResult<T>(semanticNamePrefix, id, "Failed to deserialize json (empty or invalid).");

        public interface IConfiguration : IApplicationConfigurationWrapper  //have some way of splitting out sections while maintaining syntactic convention 
        {
            bool AllowInternalReintialize { get; }

            //Publishers (& Notifiers)
            string PublishDirectoryRoot { get; }
            string DefaultDataFileName { get; }
            string DefaultMetadataFileName { get; }
            bool AllowOverwriteExistingNotification { get; } //default to false 
            //End Publishers (& Notifiers)

            //Notifiers
            string NotificationsDirectoryName { get; } //path?
            string MetadataFileName { get; }
            string DataFileName { get; }
            MyNotifier.Notifiers.AllowedNotificationTypeArgs AllowedNotificationTypeArgs { get; }
            int DisconnectionAttemptsCount { get; }
            int TryDisconnectLoopDelayMs { get; }
            int NotificationPollingDelayMs { get; }
            bool DeleteNotificationOnDelivered { get; }
            int[] RetrySequenceDelaysMs { get; set; } //not to worry about bottlenecking performance, since running full sequence should really only occur during actual exceptions. will handle oversized file cases later
            //WriteCompleteSignalArgs WriteCompleteSignalArgs { get; }
            TimeSpan ClearCacheInterval { get; }
            //End Notifiers 
        }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public bool AllowInternalReintialize => throw new NotImplementedException();

            //Publishers
            public string PublishDirectoryRoot => throw new NotImplementedException();
            public string DefaultDataFileName => throw new NotImplementedException();
            public string DefaultMetadataFileName => throw new NotImplementedException();
            public bool AllowOverwriteExistingNotification => throw new NotImplementedException();

            public string NotificationsDirectoryName => throw new NotImplementedException();

            public string MetadataFileName => throw new NotImplementedException();

            public string DataFileName => throw new NotImplementedException();

            public MyNotifier.Notifiers.AllowedNotificationTypeArgs AllowedNotificationTypeArgs => throw new NotImplementedException();

            public int DisconnectionAttemptsCount => throw new NotImplementedException();

            public int TryDisconnectLoopDelayMs => throw new NotImplementedException();

            public int NotificationPollingDelayMs => throw new NotImplementedException();

            public bool DeleteNotificationOnDelivered => throw new NotImplementedException();

            public int[] RetrySequenceDelaysMs { get; set; } = [5000, 10000, 30000, 60000]; //not to worry about bottlenecking performance, since running full sequence should really only occur during actual exceptions. will handle oversized file cases later

            //public WriteCompleteSignalArgs WriteCompleteSignalArgs => throw new NotImplementedException();

            public TimeSpan ClearCacheInterval => throw new NotImplementedException();

            //End Publishers

            //Notifiers

            //End Notifiers

            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration) { }

        }
    }
}

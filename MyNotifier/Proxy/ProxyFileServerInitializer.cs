using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Contracts.Proxy;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using static MyNotifier.Updaters.Updater;
using static MyNotifier.Proxy.ProxyFileServerInitializer;
using System.Runtime.CompilerServices;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Updaters;
using MyNotifier.Publishers;
using MyNotifier.Contracts.Publishers;
using MyNotifier.Notifiers;
using MyNotifier.FileIOManager;
using System.IO;
using IUpdaterFactory = MyNotifier.Contracts.Updaters.IFactory;
using UpdaterFactory = MyNotifier.Updaters.Factory;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;
using CustomUpdaterDefinition = MyNotifier.Contracts.Updaters.CustomDefinition;

namespace MyNotifier.Proxy
{
    public class ProxyFileServerInitializer : NotifierSystemInitializer<Args>, IProxyFileServerInitializer  //proxyFileServer-scheme system initializer //can get proxySettings from config or initialize() args //THIS NEEDS WORK !!! 
    {

        private const string ProxyInitializationFailedMessageFormat = "Proxy file server initialization failed: {0}";
        private const string BadProxySettingsMessage = "Missing or invalid configuration proxy settings.";

        private readonly IProxyFileServerInitializerIOManager proxyIOManager;
        private readonly IConfiguration configuration;
        private readonly ICallContext<ProxyFileServerInitializer> callContext;

        public ProxyFileServerInitializer(IProxyFileServerInitializerIOManager proxyIOManager, ICallContext<ProxyFileServerInitializer> callContext) : base() { this.proxyIOManager =  proxyIOManager; this.callContext = callContext; }
        public ProxyFileServerInitializer(IProxyFileServerInitializerIOManager proxyIOManager, IConfiguration configuration, ICallContext<ProxyFileServerInitializer> callContext) : this(proxyIOManager, callContext) { this.configuration = configuration; }

        //using construction-time config
        protected override async ValueTask<IResult> InitializeSystemCoreAsync() //should receive application configuration/ appConfig:NotifierSystemInit section as init arg, shouldn't be pulling values out of ctor config 
        {
            if (this.configuration == null || this.configuration.ProxySettings == null) return new Result(false, BadProxySettingsMessage);

            return await this.InitializeSystemCoreAsync(this.configuration.ProxySettings);
        }

        protected override async ValueTask<IResult> InitializeSystemCoreAsync(Args args) => await this.InitializeSystemCoreAsync(args.ProxySettings).ConfigureAwait(false);

        private async ValueTask<IResult> InitializeSystemCoreAsync(IProxySettings proxySettings = null)
        {
            try
            {
                //read id -> defn map "catalog" file 
                //locate & download intereset definitions 
                //locate & download updater dlls based on active session interests
                //reflectively activate relevant updaters, add to session service collection later
                
                //maybe should have dedicated method for setting proxySetting, could include validation 
                var proxyIOManagerInitializeResult = await this.proxyIOManager.InitializeAsync(proxySettings ?? this.configuration.ProxySettings, true).ConfigureAwait(false); 
                if (!proxyIOManagerInitializeResult.Success) return BuildFailedResult(proxyIOManagerInitializeResult);

                var validateSystemProxyResult = await this.proxyIOManager.EnsureProxyFileSystemAsync().ConfigureAwait(false); if (!validateSystemProxyResult.Success) return BuildFailedResult(validateSystemProxyResult);

                var loadInterestModelsResult = await this.proxyIOManager.LoadInterestModelsAsync().ConfigureAwait(false); if (!loadInterestModelsResult.Success) return BuildFailedResult(loadInterestModelsResult);
                var loadCatalogResult = await this.proxyIOManager.LoadCatalogAsync().ConfigureAwait(false); if (!loadCatalogResult.Success) return BuildFailedResult(loadCatalogResult);

                //loading dlls should maybe be handled by driver.initializer 
                var validateAndInitializeForUpdaterDefinitionsResult = await this.proxyIOManager.ValidateAndInitializeForKnownUpdaterDefinitionsAsync(loadInterestModelsResult.Result, loadCatalogResult.Result).ConfigureAwait(false);
                if (!validateAndInitializeForUpdaterDefinitionsResult.Success) return BuildFailedResult(validateAndInitializeForUpdaterDefinitionsResult);
                //validateUpdaters() //same as above, also match with interests if necessary 

                //should probably be handled by driver initializer, maybe this provides additions to service collection. For now, will handle here 
                var applicationServiceCollection = new ServiceCollection();
                new Registrar().RegisterServices(applicationServiceCollection, 
                                                 loadCatalogResult.Result, 
                                                 validateAndInitializeForUpdaterDefinitionsResult.Result, 
                                                 this.configuration);

                return new Result() //result ?? depends on scheme, must abstract. should probably be handled by driver.initializer 
                {
                    Catalog = loadCatalogResult.Result,
                    InterestModels = loadInterestModelsResult.Result,
                    ServiceProvider = applicationServiceCollection.BuildServiceProvider()
                };
            }
            catch (Exception ex) { return Result.FromFailedCallResult(CallResult.FromException(ex)); }
        }

        //refactor these 
        private static IResult BuildFailedResult(ICallResult innerCallResult) => Result.FromFailedCallResult(CallResult.BuildFailedCallResult(innerCallResult, ProxyInitializationFailedMessageFormat));
        private static IResult BuildFailedResult<T>(ICallResult<T> innerCallResult) => Result.FromFailedCallResult(CallResult<T>.BuildFailedCallResult(innerCallResult, ProxyInitializationFailedMessageFormat));


        public new class Args : NotifierSystemInitializer.Args
        {
            IApplicationConfiguration ApplicationConfiguration { get; set; }
            public IProxySettings ProxySettings { get; set; }
        }

        public new interface IConfiguration : NotifierSystemInitializer.IConfiguration //proxy settings are not meaningful at this layer !!! initializer should just use IApplicationConfiguration !!!
        {
            IProxySettings ProxySettings { get; }
        }
        public new class Configuration : NotifierSystemInitializer.Configuration, IConfiguration
        {
            private IProxySettings proxySettings;

            public IProxySettings ProxySettings => this.proxySettings;

            public Configuration(IApplicationConfiguration applicationConfiguration) : base(applicationConfiguration) => this.BuildAndValidate();
        
        
            private void BuildAndValidate()
            {

                if (this.InnerApplicationConfiguration.SystemSettings.Scheme != SystemScheme.ProxyFileIOServer) throw new Exception("Invalid system scheme from application confiugration.");

                IProxySettings? proxySettings = this.InnerApplicationConfiguration.SystemSettings.Settings as IProxySettings ?? throw new Exception("Invalid proxy settings from application configuration.");

                this.proxySettings = proxySettings;
            }
        }


        protected class Registrar
        {
            public void RegisterServices(IServiceCollection services, 
                                         Catalog catalog, 
                                         HashSet<UpdaterDefinitionModel> updaterDefinitions,
                                         IConfiguration configuration)
            {
                services.AddSingleton(configuration.InnerConfiguration);
                services.AddSingleton(configuration.InnerApplicationConfiguration);
                services.AddSingleton(catalog);

                //register command object 

                //file IO Manager 
                this.RegisterFileIOManagerServices(services, configuration);
                //notifier publisher / server notifier 
                this.RegisterNotifierPublisherAndServerNotifierServices(services);
                //updaters 
                this.RegisterUpdaterServices(services, updaterDefinitions);
            }

            //register transients or scopeds ?? make configurable ? 
            private void RegisterFileIOManagerServices(IServiceCollection services, IConfiguration configuration) //register transients? ioManagers differ by context ? maybe have factory to swap in and out different configs. cache should be common 
            {
                switch (configuration.ProxySettings.ProxyHost)
                {
                    case FileStorageProvider.Local:

                        services.AddSingleton<LocalDriveFileIOManager.IConfiguration, LocalDriveFileIOManager.Configuration>();

                        services.AddScoped<ICallContext<LocalDriveFileIOManager>, CallContext<LocalDriveFileIOManager>>();
                        services.AddScoped<IFileIOManager, LocalDriveFileIOManager>();

                        break;
                    case FileStorageProvider.GoogleDrive:

                        services.AddSingleton<GoogleDriveFileIOManager.IConfiguration, GoogleDriveFileIOManager.Configuration>();

                        //id cache?
                        //if ioManagers are registered as transients, cache should still be shared 

                        services.AddScoped<ICallContext<GoogleDriveFileIOManager>, CallContext<GoogleDriveFileIOManager>>();
                        services.AddScoped<IFileIOManager, GoogleDriveFileIOManager>();

                        break;
                    //default: throw new Exception("Invalid FileStorageProvider."); //should never be reached
                }
            }

            private void RegisterNotifierPublisherAndServerNotifierServices(IServiceCollection services)
            {
                //services.AddScoped<INotificationFileSystemObjectTranslator, DefaultTranslator>();

                this.RegisterNotifierPublisherServices(services);
                this.RegisterServerNotifierServices(services);
            }

            private void RegisterUpdaterServices(IServiceCollection services, HashSet<UpdaterDefinitionModel> updaterDefinitions)
            {
                services.AddScoped<UpdaterFactory.IConfiguration, UpdaterFactory.Configuration>();
                services.AddScoped<ICallContext<UpdaterFactory>, CallContext<UpdaterFactory>>();
                services.AddScoped<IUpdaterFactory, UpdaterFactory>();

                foreach (var updaterDefinition in updaterDefinitions)
                {       
                                                       //encapsulate, maybe just provide serviceDescriptor
                    var updaterAssemblyQualifiedName = BuildAssemblyQualifiedTypeNames(updaterDefinition); //encapsulate BuildAQN somewhere ?
                    var updaterType = Type.GetType(updaterAssemblyQualifiedName) ?? throw new Exception($"Encountered null updater type: {updaterAssemblyQualifiedName}"); //shouldn't happen, types have already been registered/confirmed by now (generally) 
                    services.AddScoped(updaterType);

                    foreach (var dependency in updaterDefinition.Dependencies) services.Add(dependency);
                }
            }

            private void RegisterNotifierPublisherServices(IServiceCollection services)
            {
                //services.AddSingleton(this.BuildNotifierPublisherConfiguration());

                //services.AddScoped<INotificationFileSystemObjectTranslator, DefaultTranslator>();
                //services.AddScoped<ICallContext<FileNotifierPublisher>, CallContext<FileNotifierPublisher>>();
                //services.AddScoped<INotifierPublisher, FileNotifierPublisher>();
            }

            private void RegisterServerNotifierServices(IServiceCollection services)
            {
                //!!! AG !!! 
                //services.AddSingleton(this.BuildNotifierConfiguration());

                //services.AddScoped<ICallContext<FileNotifier>, CallContext<FileNotifier>>();
                //services.AddScoped<IFileNotifier, FileNotifier>();
            }

            //private FileNotifierPublisher.IConfiguration BuildNotifierPublisherConfiguration()  //still have to figure out config wire-up + update on command 
            //{

            //    var config = new FileNotifierPublisher.Configuration(null)
            //    {

            //    };


            //    return config;
            //}

            //private FileNotifier.IConfiguration BuildNotifierConfiguration()
            //{
            //    throw new NotImplementedException();
            //}

            //encapsulate! 
            private static string BuildAssemblyQualifiedTypeNames(UpdaterDefinitionModel updaterDefinition) => string.Format("{0}.{1}", updaterDefinition.ModuleDescription.AssemblyName, updaterDefinition.ModuleDescription.TypeFullName);
        }

        #region IOManager
        public interface IProxyFileServerInitializerIOManager 
        {
            IProxySettings ProxySettings { get; } //maybe should have dedicated Set Method ? //idk maybe should not be exposed 
            Task<ICallResult> InitializeAsync(IProxySettings proxySettings, bool forceReInitialize = false);
            ValueTask<ICallResult> EnsureProxyFileSystemAsync();
            ValueTask<ICallResult<Catalog>> LoadCatalogAsync();
            ValueTask<ICallResult<InterestModel[]>> LoadInterestModelsAsync();
            ValueTask<ICallResult<HashSet<UpdaterDefinitionModel>>> ValidateAndInitializeForKnownUpdaterDefinitionsAsync(InterestModel[] interests, Catalog catalog);
        }

        public class ProxyFileServerInitializerIOManager : IProxyFileServerInitializerIOManager
        {

            private const string ValidateFailedMessageFormat = "Validate proxy failed: {0}";
            private const string ProxySettingsAbsentOrInvalidMessageFormat = "Proxy Settings Invalid {0}";
            private const string NotInitializedMessage = "Not Initialized";

            private readonly IFileIOManager fileIOManager;
            private readonly ICallContext<ProxyFileServerInitializerIOManager> callContext;

            private Encoding defaultEncoding = Encoding.UTF8; //make configurable 

            private IProxySettings proxySettings = null;
            private Paths paths;

            private bool isInitialized = false;

            private ModuleLoader moduleLoader;

            public IProxySettings ProxySettings => this.proxySettings;

            public ProxyFileServerInitializerIOManager(IFileIOManager fileIOManager, ICallContext<ProxyFileServerInitializerIOManager> callContext) { this.fileIOManager = fileIOManager; this.callContext = callContext; }


            public async Task<ICallResult> InitializeAsync(IProxySettings proxySettings, bool forceReInitialize = false)
            {
                try
                {
                    if (!this.isInitialized || forceReInitialize)
                    {
                        var validateProxySettingsResult = this.ValidateProxySettings(); if (!validateProxySettingsResult.Success) return validateProxySettingsResult;

                        var fileIOManagerInitResult = await this.fileIOManager.InitializeAsync().ConfigureAwait(false);
                        if (!fileIOManagerInitResult.Success) return CallResult.BuildFailedCallResult(fileIOManagerInitResult, "Failed to initialize fileIOManager");

                        this.paths = proxySettings.FileStructure.BuildPaths(new FileIOManager.FileIOManager.Wrapper(this.fileIOManager));
                        this.moduleLoader = new ModuleLoader(this.fileIOManager, this.paths);

                        this.isInitialized = true;
                    }

                    return new CallResult();
                }
                catch (Exception ex) { return CallResult.FromException(ex); }
            }


            //system does not always use a proxy, but for now... 
            public async ValueTask<ICallResult> EnsureProxyFileSystemAsync()
            {
                try
                {
                    //if (!this.proxySetingsValidated) return new CallResult(false, "Proxy settings not validated.");
                    if (!this.isInitialized) return new CallResult(false, NotInitializedMessage);

                    //folder names should be in config !!! 
                    //encapsulate path building 
                    //put exceptions in notifications folder (as notification)
                    //assert dlls folder not empty

                    foreach (var fileSystemObject in this.paths)
                    {
                        var ensureOrAssertResult = await this.EnsureOrAssertProxyFolderExistsAsync(fileSystemObject.Path,
                                                                                                   fileSystemObject.SemanticName,
                                                                                                   !fileSystemObject.Assert && this.proxySettings.AllowRecreateProxyFileSystem).ConfigureAwait(false);

                        if (!ensureOrAssertResult.Success) return CallResult.BuildFailedCallResult(ensureOrAssertResult, ValidateFailedMessageFormat);
                    }


                    //HANDLED ABOVE 
                    ////catalog file
                    //var assertCatalogFileExistsResult = await this.AssertFileExistsAsync(this.paths.CatalogFile.Path, this.paths.CatalogFile.SemanticName).ConfigureAwait(false);
                    //if (!assertCatalogFileExistsResult.Success) return CallResult.BuildFailedCallResult(assertCatalogFileExistsResult, ValidateFailedMessageFormat);

                    ////interests file, not sure if needed 
                    //var assertInterestsFileExistsResult = await this.AssertFileExistsAsync(this.paths.InterestsFile.Path, this.paths.InterestsFile.SemanticName).ConfigureAwait(false);
                    //if (!assertInterestsFileExistsResult.Success) return CallResult.BuildFailedCallResult(assertInterestsFileExistsResult, ValidateFailedMessageFormat);

                    return new CallResult();
                }
                catch (Exception ex) { return CallResult.FromException(ex); }
            }

            public async ValueTask<ICallResult<Catalog>> LoadCatalogAsync()
            {
                try
                {
                    if (!this.isInitialized) return new CallResult<Catalog>(false, NotInitializedMessage);

                    var loadCatalogResult = await this.LoadAsync<Catalog>(this.paths.CatalogFile.Path, this.paths.CatalogFile.SemanticName).ConfigureAwait(false);
                    if (!loadCatalogResult.Success) return CallResult<Catalog>.BuildFailedCallResult(loadCatalogResult, "Failed to load catalog");

                    return loadCatalogResult;
                }
                catch (Exception ex) { return CallResult<Catalog>.FromException(ex); }
            }

            public async ValueTask<ICallResult<InterestModel[]>> LoadInterestModelsAsync() 
            {
                try
                {
                    if (!this.isInitialized) return new CallResult<InterestModel[]>(false, NotInitializedMessage);

                    var loadCatalogResult = await this.LoadAsync<InterestModel[]>(this.paths.CatalogFile.Path, this.paths.CatalogFile.SemanticName).ConfigureAwait(false);
                    if (!loadCatalogResult.Success) return CallResult<InterestModel[]>.BuildFailedCallResult(loadCatalogResult, "Failed to load catalog");

                    return loadCatalogResult;
                }
                catch (Exception ex) { return CallResult<InterestModel[]>.FromException(ex); }
            }

            public async ValueTask<ICallResult<HashSet<UpdaterDefinitionModel>>> ValidateAndInitializeForKnownUpdaterDefinitionsAsync(InterestModel[] interests, Catalog catalog)
            {
                try
                {
                    if (!this.isInitialized) return new CallResult<HashSet<UpdaterDefinitionModel>>(false, NotInitializedMessage);

                    InterestModel[] interestModels = [];

                    var ret = new HashSet<UpdaterDefinitionModel>();

                    await foreach(var loadModuleResult in this.moduleLoader.LoadModulesAsync(interestModels, catalog).ConfigureAwait(false))
                    {
                        //on load module failed? 
                        if (!loadModuleResult.Success) { return CallResult<HashSet<UpdaterDefinitionModel>>.BuildFailedCallResult((ICallResult<HashSet<UpdaterDefinitionModel>>)loadModuleResult, "Failed to load and validate module"); }
                        ret.Add(loadModuleResult.Result);
                    }

                    return new CallResult<HashSet<UpdaterDefinitionModel>>(ret);
                }
                catch (Exception ex) { return CallResult<HashSet<UpdaterDefinitionModel>>.FromException(ex); }
            }

            private ICallResult ValidateProxySettings()
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

            private async Task<ICallResult> EnsureOrAssertProxyFolderExistsAsync(string folderPath, string semanticFolderName, bool assert)
            {
                try
                {
                    var folderExistsResult = await this.fileIOManager.DirectoryExistsAsync(folderPath).ConfigureAwait(false);
                    if (!folderExistsResult.Success) return new CallResult(false, $"{semanticFolderName} exists call failed: {folderExistsResult.ErrorText}");

                    if (!folderExistsResult.Result)
                    {
                        if (!assert) return new CallResult(false, $"Proxy not configured. reconfiguration not allowed/possible. [{semanticFolderName} missing]");

                        var createFolderResult = await this.fileIOManager.CreateDirectoryAsync(folderPath).ConfigureAwait(false);
                        if (!createFolderResult.Success) return new CallResult(false, $"Create {semanticFolderName} call failed: {createFolderResult.ErrorText}");
                    }

                    return new CallResult();
                }
                catch (Exception ex) { return new CallResult(false, ex.Message); }
            }

            private async Task<ICallResult<T>> LoadAsync<T>(string filePath, string objectSemanticName)  //doesn't handle encoding !!! 
            {
                try
                {
                    if (!this.isInitialized) return new CallResult<T>(false, NotInitializedMessage);

                    var createReadStreamResult = this.fileIOManager.CreateReadFileStream(filePath);
                    if (!createReadStreamResult.Success) return new CallResult<T>(false, $"Create {objectSemanticName} file read stream call failed: {createReadStreamResult.ErrorText}");

                    using var sr = new StreamReader(createReadStreamResult.Result, this.defaultEncoding);

                    var json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    var obj = JsonSerializer.Deserialize<T>(json);

                    if (obj == null) return new CallResult<T>(false, $"loaded object ({objectSemanticName}) is null.");

                    return new CallResult<T>(obj);
                }
                catch (Exception ex) { return new CallResult<T>(false, ex.Message); }
            }

            //private async Task<ICallResult> AssertFileExistsAsync(string filePath, string semanticName)
            //{
            //    try
            //    {
            //        var result = await this.fileIOManager.FileExistsAsync(filePath).ConfigureAwait(false);

            //        if (!result.Success) return new CallResult(false, $"{semanticName} file exists call failed: {result.ErrorText}");
            //        if (!result.Result) return new CallResult(false, $"Required {semanticName} file does not exist on proxy");

            //        return new CallResult();
            //    }
            //    catch (Exception ex) { return new CallResult(false, ex.Message); }
            //}          


            protected class ModuleLoader
            {
                private readonly IFileIOManager fileIOManager;
                private readonly IFileIOManager.IWrapper fileIOManagerWrapper;
                private readonly Paths paths;

                public ModuleLoader(IFileIOManager fileIOManager, Paths paths)
                {
                    this.fileIOManager = fileIOManager; 
                    this.fileIOManagerWrapper = new FileIOManager.FileIOManager.Wrapper(fileIOManager);
                    this.paths = paths;
                }

                public async IAsyncEnumerable<ILoadModuleResult> LoadModulesAsync(InterestModel[] interestModels, Catalog catalog)
                {
                    //interests -> updaters -> assemblies 
                    //assemblies in library/dlls 
                    //load relevant assemblies 

                    //should not have duplicate interest models 
                    var processedEventModuleModelIds = new HashSet<Guid>();
                    var processedUpdaterDefinitionIds = new HashSet<Guid>();

                    foreach (var interestModel in interestModels)
                    {
                        foreach (var eventModuleModel in interestModel.EventModuleModels)
                        {
                            if (processedEventModuleModelIds.Contains(eventModuleModel.Definition.Id)) continue;

                            string modulePath;
                            byte[] moduleBytes;
                            Assembly assembly;

                            foreach (var updaterDefinition in eventModuleModel.Definition.UpdaterDefinitions)
                            {
                                ILoadModuleResult ret;
                                try //this gets kind of ugly with nested conditionals, could be refactored. goes back to fileIOManager not returning ICallResults
                                {
                                    if (processedUpdaterDefinitionIds.Contains(updaterDefinition.Id)) continue;

                                    modulePath = this.BuildModulePath(updaterDefinition.ModuleDescription);
                                    var createDllReadStreamResult = this.fileIOManager.CreateReadFileStream(modulePath);

                                    if (createDllReadStreamResult.Success) 
                                    {
                                        using var dllReadStream = createDllReadStreamResult.Result;
                                        using var ms = new MemoryStream();

                                        await dllReadStream.CopyToAsync(ms).ConfigureAwait(false);

                                        moduleBytes = ms.ToArray();     //moduleBytes = await this.LoadDllBytesFromPathAsync(modulePath).ConfigureAwait(false);
                                        assembly = Assembly.Load(moduleBytes);

                                        if (!this.TryValidateAssemblyForUpdaterDefinition(assembly, updaterDefinition, out var validateAssemblyErrorText)) ret = new LoadModuleResult() { Success = false, ErrorText = validateAssemblyErrorText };
                                        else 
                                        {
                                            processedUpdaterDefinitionIds.Add(updaterDefinition.Id);
                                            ret = new LoadModuleResult { Result = updaterDefinition };
                                        }
                                    }
                                    else { ret = new LoadModuleResult() { Success = false, ErrorText = $"Failed to create module read stream for updater: {updaterDefinition.Id}-{updaterDefinition.Name} at {modulePath}" }; }
                                } catch (Exception ex) { ret = new LoadModuleResult() { Success = false, ErrorText = ex.Message }; }

                                yield return ret;
                            }

                            processedEventModuleModelIds.Add(eventModuleModel.Definition.Id);
                        }
                    }
                }

                private string BuildModulePath(IModuleDescription moduleDescription) => this.fileIOManagerWrapper.BuildAppendedPath(this.paths.DllsFolder.Path, this.BuildModuleName(moduleDescription));
                private string BuildModuleName(IModuleDescription moduleDescription) => $"{moduleDescription}.dll";

                //private async Task<byte[]> LoadDllBytesFromPathAsync(string path)
                //{
                //    var createDllReadStreamResult = this.fileIOManager.CreateReadFileStream(path);
                //    if (!createDllReadStreamResult.Success)
                //        return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: failed to create read stream for dll {path}: {createDllReadStreamResult.ErrorText}");

                //    using var dllReadStream = createDllReadStreamResult.Result;
                //    using var ms = new MemoryStream();

                //    await dllReadStream.CopyToAsync(ms).ConfigureAwait(false);
                    
                //    return ms.ToArray();
                //}

                private bool TryValidateAssemblyForUpdaterDefinition(Assembly assembly, 
                                                                     UpdaterDefinitionModel updaterDefinitionModel, 
                                                                     out string errorText)
                {
                    try
                    {
                        //would cache processedTypes for later updaterModule loads but no guarantee of assembly name being unique. no obvious cache key property 
                        //var assemblyNameToUpdaterIdUpdaterTypeMap = new Dictionary<string, HashSet<Guid>>();  //no cache key for updater types (can't use type name on off chance of collision. in practice, probably would be fine, but... Same with assembly names
                        //var assemblyNameToUpdaterIdUpdaterDefinitionTypeMaps = new Dictionary<string, HashSet<Guid>>();

                        if (assembly == null) { errorText = "Encountered null assembly."; return false; }
                        if (string.IsNullOrEmpty(assembly.FullName)) { errorText = "Encountered assembly with null name."; return false; }

                        var updaterTypePresentAndRecognized = false;
                        var updaterDefinitionTypePresentAndRecognized = false;

                        foreach (var definedType in assembly.DefinedTypes)  
                        {
                            if (definedType.FullName == updaterDefinitionModel.ModuleDescription.TypeFullName && ((Type.GetType($"{updaterDefinitionModel.ModuleDescription.AssemblyName}.{updaterDefinitionModel.ModuleDescription.TypeFullName}") != null))) { updaterTypePresentAndRecognized = true; continue; }
                            if (definedType.FullName == updaterDefinitionModel.ModuleDescription.DefinitionTypeFullName && ((Type.GetType($"{updaterDefinitionModel.ModuleDescription.AssemblyName}.{updaterDefinitionModel.ModuleDescription.DefinitionTypeFullName}") != null))) { updaterDefinitionTypePresentAndRecognized = true; continue; }
                        }

                        if (!(updaterTypePresentAndRecognized && updaterDefinitionTypePresentAndRecognized)) 
                        {
                            errorText = $"Could not find/recognize required types for updater: {updaterDefinitionModel.Id}-{updaterDefinitionModel.Name} in assembly: {assembly.FullName}"; 
                            return false;
                        }

                        errorText = string.Empty;
                        return true;

                    } catch(Exception ex) { errorText = ex.Message; return false; }
                }

                //private bool TryFindDllsFolderCollectUpdaterDirectories(string[] updaterDirectoryContents, out string[] updaterDirectories)
                //{
                //    var udList = new List<string>();
                //    var dllFolderFound = false;

                //    foreach (var ud in updaterDirectoryContents)
                //    {
                //        if (ud == this.paths.DllsFolder.Name) dllFolderFound = true;
                //        else udList.Add(ud);
                //    }

                //    updaterDirectories = [.. udList];

                //    return dllFolderFound;
                //}


                public interface ILoadModuleResult : ICallResult<UpdaterDefinitionModel> { }
                public class LoadModuleResult : CallResult<UpdaterDefinitionModel>, ILoadModuleResult { }
            }

            private static CallResult BuildFailedCallResult(ICallResult innerCallResult) => CallResult.BuildFailedCallResult(innerCallResult, ProxyInitializationFailedMessageFormat);
            private static CallResult<T> BuildFailedCallResult<T>(ICallResult<T> innerCallResult) => CallResult<T>.BuildFailedCallResult(innerCallResult, ProxyInitializationFailedMessageFormat);


            //protected class OldDllLoaderLogic
            //{
            //    //validate known updater subdirectories [updater dll folder, state folder], validate represented updaters exist, load updaters
            //    //should never encounter duplicate assemblies 
            //    //one assembly can contain multiple updaters, need to map updater types to respective assemblies !!! 
            //    //load updater dlls as needed, or all together? 

            //    //THIS MUST BE REWORKED !!! 
            //    private async Task<ICallResult<HashSet<IUpdaterDefinition>>> ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync(Interest[] interests, Catalog catalog)  //validate updaters load known dlls 
            //    {
            //        try
            //        {
            //            var updaterDirectoriesResult = await this.fileIOManager.GetDirectoriesAsync(this.paths.UpdatersFolder).ConfigureAwait(false);
            //            if (updaterDirectoriesResult.Success) return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: {updaterDirectoriesResult.ErrorText}");

            //            var dllFolderFound = this.TryFindDllsFolderCollectUpdaterDirectories(updaterDirectoriesResult.Result, out var updaterDirectories);

            //            //maybe crash here? can't work without updaters/updater dlls 
            //            if (!dllFolderFound)
            //            {
            //                return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: No updater dll folder exists.");

            //                //if (this.proxySettings.AllowRecreateProxyFileSystem)
            //                //{
            //                //    //encapsulate combine path !! 
            //                //    var createDllFolderResult = await this.fileIOManager.CreateDirectoryAsync(this.proxySettings.UpdatersFolderPath + "/" + DllsFolderName).ConfigureAwait(false);
            //                //    if (!createDllFolderResult.Success) return new CallResult<Assembly[]>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: failed to create dlls folder: {createDllFolderResult.ErrorText}");
            //                //}
            //                //else return new CallResult<Assembly[]>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: No updater dll folder found, reconfig not allowed.");
            //            }


            //            var updaterDirectoriesSet = new HashSet<string>(updaterDirectories);
            //            var validatedUpdaterDirectories = new HashSet<Guid>();

            //            //maps updater assembly names to updaters using assembly
            //            var assemblyNameUpdaterDefinitionMap = new Dictionary<string, Dictionary<Guid, IUpdaterDefinition>>();

            //            foreach (var interest in interests)
            //            {
            //                foreach (var updaterDefinition in interest.UpdaterArgs.Definitions)
            //                {

            //                    if (!validatedUpdaterDirectories.Contains(updaterDefinition.Id))
            //                    {
            //                        //assert associated updater folder exists 
            //                        if (!updaterDirectoriesSet.Contains(updaterDefinition.Id.ToString())) //encapsulate updater -> updaterFolderName translation, for now folderName = updater.Id
            //                        {
            //                            if (this.proxySettings.AllowRecreateProxyFileSystem)
            //                            {
            //                                //create updater directory 
            //                                var createUpdaterDirectoryResult = await this.fileIOManager.CreateDirectoryAsync(this.proxySettings.UpdatersFolderName + "/" + updaterDefinition.Id.ToString()).ConfigureAwait(false);
            //                                if (!createUpdaterDirectoryResult.Success) return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: failed to create updater directory for updater: {updaterDefinition.Id}.");
            //                            }
            //                            else return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: no updater for interest: {interest.Definition.Id} expected: {updaterDefinition.Id}. Proxy server reconfiguration disallowed.");
            //                        }

            //                        //state folder...or file?? probably file would suffice but for now will leave folder 
            //                        var stateFolderPath = updaterDefinition.Id.ToString() + '/' + "State";
            //                        //var dllFolderPath = updaterDefinition.Id.ToString() + '/' + "Dlls";

            //                        //ensure state folder exists 
            //                        var stateFolderExistsResult = await this.fileIOManager.DirectoryExistsAsync(stateFolderPath).ConfigureAwait(false);
            //                        if (!stateFolderExistsResult.Success)
            //                            return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: State folder exists call failed: {stateFolderExistsResult.ErrorText} for updater {updaterDefinition.Id}.");

            //                        if (!stateFolderExistsResult.Result)
            //                        {
            //                            if (!this.proxySettings.AllowRecreateProxyFileSystem)
            //                                return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: State folder does not exist and proxy server reconfiguration disallowed.");

            //                            var createStateFolderResult = await this.fileIOManager.CreateDirectoryAsync(stateFolderPath).ConfigureAwait(false);
            //                            if (!createStateFolderResult.Success)
            //                                return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: State folder creation call failed: {createStateFolderResult.ErrorText}.");
            //                        }

            //                        if (assemblyNameUpdaterDefinitionMap.TryGetValue(updaterDefinition.ModuleDescription.AssemblyName, out var updaterDefinitionMap)) updaterDefinitionMap.TryAdd(updaterDefinition.Id, updaterDefinition);
            //                        else { assemblyNameUpdaterDefinitionMap.Add(updaterDefinition.ModuleDescription.AssemblyName, new Dictionary<Guid, IUpdaterDefinition>() { { updaterDefinition.Id, updaterDefinition } }); }
            //                    }
            //                }
            //            }


            //            //DLLs
            //            // Load All v. Load For Known Interest defintions v. load for session interests 
            //            //for now, will load for all known interest definitions only

            //            var loadedUpdaterDefinitions = new HashSet<IUpdaterDefinition>();

            //            var getDllFilesResult = await this.fileIOManager.GetFilesAsync(this.paths.DllsFolder.Path).ConfigureAwait(false); //could use fileExists per updaterDefinition instead 
            //            if (!getDllFilesResult.Success) return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: Could not load Dll files: {getDllFilesResult.ErrorText}");

            //            var dllFiles = new HashSet<string>(getDllFilesResult.Result);

            //            foreach (var kvp in assemblyNameUpdaterDefinitionMap)
            //            {
            //                var requiredDllFile = kvp.Key + ".dll";

            //                if (!dllFiles.Contains(requiredDllFile)) return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: missing required dll file: {requiredDllFile}");

            //                var createDllReadStreamResult = this.fileIOManager.CreateReadFileStream(requiredDllFile);
            //                if (!createDllReadStreamResult.Success)
            //                    return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} failed: failed to create read stream for dll {requiredDllFile}: {createDllReadStreamResult.ErrorText}");


            //                byte[] dllBytes;
            //                using (var dllReadStream = createDllReadStreamResult.Result)
            //                {
            //                    using var ms = new MemoryStream();

            //                    await dllReadStream.CopyToAsync(ms).ConfigureAwait(false);
            //                    dllBytes = ms.ToArray();
            //                }

            //                var assembly = Assembly.Load(dllBytes);

            //                if (string.IsNullOrEmpty(assembly.FullName)) throw new Exception("Assembly encountered with null name.");

            //                foreach (var updaterDefinition in kvp.Value.Values)
            //                {
            //                    var updaterTypePresentAndRecognized = false;
            //                    var updaterDefinitionTypePresentAndRecognized = false;

            //                    foreach (var definedType in assembly.DefinedTypes)
            //                    {
            //                        if (definedType.FullName == updaterDefinition.ModuleDescription.TypeFullName &&
            //                            ((Type.GetType($"{updaterDefinition.ModuleDescription.AssemblyName}.{updaterDefinition.ModuleDescription.TypeFullName}") != null))) { updaterTypePresentAndRecognized = true; continue; }
            //                        if (definedType.FullName == updaterDefinition.ModuleDescription.DefinitionTypeFullName &&
            //                            ((Type.GetType($"{updaterDefinition.ModuleDescription.AssemblyName}.{updaterDefinition.ModuleDescription.DefinitionTypeFullName}") != null))) { updaterDefinitionTypePresentAndRecognized = true; continue; }
            //                    }
            //                    if (!(updaterTypePresentAndRecognized && updaterDefinitionTypePresentAndRecognized)) return new CallResult<HashSet<IUpdaterDefinition>>(false, $"{nameof(ValidateUpdatersLoadKnownUpdaterDllsForInterestsAsync)} could not find/recognize relevant types in assembly for updater {updaterDefinition.Id}");

            //                    loadedUpdaterDefinitions.Add(updaterDefinition);
            //                }
            //            }


            //            return new CallResult<HashSet<IUpdaterDefinition>>(loadedUpdaterDefinitions);

            //        }
            //        catch (Exception ex) { return new CallResult<HashSet<IUpdaterDefinition>>(false, ex.Message); }
            //    }


            //    private bool TryFindDllsFolderCollectUpdaterDirectories(string[] updaterDirectoryContents, out string[] updaterDirectories)
            //    {
            //        var udList = new List<string>();
            //        var dllFolderFound = false;

            //        foreach (var ud in updaterDirectoryContents)
            //        {
            //            if (ud == this.paths.DllsFolder.Name) dllFolderFound = true;
            //            else udList.Add(ud);
            //        }

            //        updaterDirectories = [.. udList];

            //        return dllFolderFound;
            //    }
            //}


            //private class FileStructure 
            //{
            //    public FileSystemObjectWrapper RootFolder { get; set; } = new() { Name = "MyNotifier", SemanticName = "Root", Assert = true };
            //        public FileSystemObjectWrapper NotificationsFolder { get; set; } = new() { Name = "Notifications", SemanticName = "Notifications", Assert = false };
            //            public FileSystemObjectWrapper UpdatesFolder { get; set; } = new() { Name = "Updates", SemanticName = "Updates", Assert = false };
            //            public FileSystemObjectWrapper CommandsFolder { get; set; } = new() { Name = "Commands", SemanticName = "Commands", Assert = false }; //command / CommandResult 
            //            public FileSystemObjectWrapper ExceptionsFolder { get; set; } = new() { Name = "Exceptions", SemanticName = "Exceptions", Assert = false };
            //        public FileSystemObjectWrapper CatalogFile { get; set; } = new() { Name = "Catalog", SemanticName = "Catalog", Assert = true };
            //        public FileSystemObjectWrapper LibraryFolder { get; set; } = new() { Name = "Library", SemanticName = "Library", Assert = true };
            //            public FileSystemObjectWrapper DllsFolder { get; set; } = new() { Name = "Dlls", SemanticName = "Dlls", Assert = true };
            //            public FileSystemObjectWrapper EventModuleDefinitionsFolder = new() { Name = "EventModuleDefinitions", SemanticName = "EventModuleDefinitions", Assert = true }; //assert here is conditional, true for now 
            //            public FileSystemObjectWrapper EventModuleModelsFolder = new() { Name = "EventModuleModels", SemanticName = "EventModuleModels", Assert = true }; //assert here is conditional, true for now 


            //    public Paths BuildPath(IFileIOManager fileIOManager, IProxySettings proxySettings) 
            //    {
            //        throw new NotImplementedException();
            //    }
            //}




            //private ValidateFolderArg[] BuildValidateFolderArgs() =>
            //[
            //    new() { FolderPath = this.paths.RootFolder, SemanticName = FileSystemSemanticNames.Root, Assert = true },
            //    new() { FolderPath = this.paths.UpdatersFolder, SemanticName = FileSystemSemanticNames.Updaters, Assert = true },
            //    new() { FolderPath = this.paths.DllsFolder, SemanticName = FileSystemSemanticNames.Dlls, Assert = true },
            //    new() { FolderPath = this.paths.UpdatersFolder, SemanticName = FileSystemSemanticNames.Notifications },
            //    new() { FolderPath = this.paths.CommandsFolder, SemanticName = FileSystemSemanticNames.Commands },
            //    new() { FolderPath = this.paths.ExceptionsFolder, SemanticName = FileSystemSemanticNames.Exceptions }
            //];


            //private class Paths
            //{
            //    public string RootFolder { get; set; }
            //    public string UpdatersFolder { get; set; }
            //    public string DllsFolder { get; set; }
            //    public string NotificationsFolder { get; set; }
            //    public string CommandsFolder { get; set; }
            //    public string ExceptionsFolder { get; set; }
            //    public string CatalogFile { get; set; }
            //    public string InterestsFile { get; set; }


            //    public static Paths Build(IFileIOManager fileIOManager, IProxySettings proxySettings)
            //    {
            //        var wrapper = new FileIOManager.FileIOManager.Wrapper(fileIOManager);

            //        var ret = new Paths() { RootFolder = proxySettings.RootFolderPath };

            //        ret.UpdatersFolder = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.UpdatersFolderName);
            //        ret.DllsFolder = wrapper.BuildAppendedPath(ret.UpdatersFolder, proxySettings.DllsFolderName);
            //        ret.NotificationsFolder = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.NotificationsFolderName);
            //        ret.CommandsFolder = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.CommandsFolderName);
            //        ret.ExceptionsFolder = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.ExceptionsFolderName);
            //        ret.CatalogFile = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.CatalogFileName);
            //        ret.InterestsFile = wrapper.BuildAppendedPath(ret.RootFolder, proxySettings.InterestsFileName);

            //        return ret;
            //    }
            //}

            //private class ValidateFolderArg
            //{
            //    public string FolderPath { get; set; }
            //    public string SemanticName { get; set; }
            //    public bool Assert { get; set; } = false;
            //}

            //public class FileSystemSemanticNames
            //{
            //    public static string Root => "Root";
            //    public static string Updaters => "Updaters";
            //    public static string Dlls => "Dlls";
            //    public static string Notifications => "Notifications";
            //    public static string Commands => "Commands";
            //    public static string Exceptions => "Exceptions";
            //}
        }

        #endregion IOManager

    }

    public interface IProxyFileServerInitializer : INotifierSystemInitializer<Args> { }
}

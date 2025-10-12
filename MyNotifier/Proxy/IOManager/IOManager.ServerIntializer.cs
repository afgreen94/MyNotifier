using MyNotifier.Base;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UpdaterDefinitionModel = MyNotifier.Contracts.Updaters.DefinitionModel;

namespace MyNotifier.Proxy
{
    public abstract partial class IOManager : IIOManager
    {
        protected const string ValidateFailedMessageFormat = "Validate proxy failed: {0}";
        protected const string ProxySettingsAbsentOrInvalidMessageFormat = "Proxy Settings Invalid {0}";

        protected Encoding defaultEncoding = Encoding.UTF8; //make configurable 

        public virtual async ValueTask<ICallResult> EnsureProxyFileSystemAsync()
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

        public virtual async ValueTask<ICallResult<ICatalog>> LoadCatalogAsync()
        {
            try
            {
                if (!this.isInitialized) return new CallResult<ICatalog>(false, NotInitializedMessage);

                var loadCatalogResult = await this.LoadAsync<Catalog>(this.paths.CatalogFile.Path, this.paths.CatalogFile.SemanticName).ConfigureAwait(false); //Catalog -> default ICatalog ... for now 
                if (!loadCatalogResult.Success) return CallResult<ICatalog>.BuildFailedCallResult(loadCatalogResult, "Failed to load catalog: {0}");

                return (ICallResult<ICatalog>)loadCatalogResult;
            }
            catch (Exception ex) { return CallResult<ICatalog>.FromException(ex); }
        }

        public virtual async ValueTask<ICallResult<InterestModel[]>> LoadInterestModelsAsync()
        {
            try
            {
                if (!this.isInitialized) return new CallResult<InterestModel[]>(false, NotInitializedMessage);

                var loadCatalogResult = await this.LoadAsync<InterestModel[]>(this.paths.CatalogFile.Path, this.paths.CatalogFile.SemanticName).ConfigureAwait(false);
                if (!loadCatalogResult.Success) return CallResult<InterestModel[]>.BuildFailedCallResult(loadCatalogResult, "Failed to load catalog: {0}");

                return loadCatalogResult;
            }
            catch (Exception ex) { return CallResult<InterestModel[]>.FromException(ex); }
        }

        public virtual async ValueTask<ICallResult<HashSet<UpdaterDefinitionModel>>> ValidateAndInitializeForKnownUpdaterDefinitionsAsync(InterestModel[] interests, ICatalog catalog)
        {
            try
            {
                if (!this.isInitialized) return new CallResult<HashSet<UpdaterDefinitionModel>>(false, NotInitializedMessage);

                InterestModel[] interestModels = [];

                var ret = new HashSet<UpdaterDefinitionModel>();

                await foreach (var loadModuleResult in this.LoadModulesAsync(interestModels, catalog).ConfigureAwait(false))
                {
                    //on load module failed? 
                    if (!loadModuleResult.Success) { return CallResult<HashSet<UpdaterDefinitionModel>>.BuildFailedCallResult((ICallResult<HashSet<UpdaterDefinitionModel>>)loadModuleResult, "Failed to load and validate module: {0}"); }
                    ret.Add(loadModuleResult.Result);
                }

                return new CallResult<HashSet<UpdaterDefinitionModel>>(ret);
            }
            catch (Exception ex) { return CallResult<HashSet<UpdaterDefinitionModel>>.FromException(ex); }
        }


        //helpers 

        protected virtual async Task<ICallResult> EnsureOrAssertProxyFolderExistsAsync(string folderPath, string semanticFolderName, bool assert)
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

        protected virtual async Task<ICallResult<T>> LoadAsync<T>(string filePath, string objectSemanticName)  //doesn't handle encoding !!! 
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



        #region Module Loading

        protected virtual async IAsyncEnumerable<ILoadModuleResult> LoadModulesAsync(InterestModel[] interestModels, ICatalog catalog)
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
                        }
                        catch (Exception ex) { ret = new LoadModuleResult() { Success = false, ErrorText = ex.Message }; }

                        yield return ret;
                    }

                    processedEventModuleModelIds.Add(eventModuleModel.Definition.Id);
                }
            }
        }

        protected virtual string BuildModulePath(IModuleDescription moduleDescription) => this.fileIOManagerWrapper.BuildAppendedPath(this.paths.DllsFolder.Path, this.BuildModuleName(moduleDescription));
        protected virtual string BuildModuleName(IModuleDescription moduleDescription) => $"{moduleDescription}.dll";

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

        protected virtual bool TryValidateAssemblyForUpdaterDefinition(Assembly assembly,
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

            }
            catch (Exception ex) { errorText = ex.Message; return false; }
        }

        public interface ILoadModuleResult : ICallResult<UpdaterDefinitionModel> { }
        public class LoadModuleResult : CallResult<UpdaterDefinitionModel>, ILoadModuleResult { }

        #endregion Module Loading 


    }
}

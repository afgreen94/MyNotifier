using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Proxy.Updaters;
using MyNotifier.Contracts.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyNotifier.Proxy.Updaters
{
    //public class IOManager : Contracts.Proxy.Updaters.IIOManager
    //{

    //    //encapsulate all this in base class 
    //    private readonly IFileIOManager fileIOManager;
    //    private readonly ICallContext<IOManager> callContext;

    //    private IFileIOManager.IWrapper fileIOManagerWrapper;
    //    //need proxySettings, eventually single class to handle all ProxyIO
    //    private IProxySettings proxySettings;
    //    private Paths paths;

    //    private bool isInitialized = false;

    //    //public IOManager(IFileIOManager fileIOManager, ICallContext<IOManager> callContext) : base()

    //    //public async Task<ICallResult> InitializeAsync(bool forceReinitialize) //encapsulate in single Proxy.IOManager class 
    //    //{
    //    //    try
    //    //    {
    //    //        if(!this.isInitialized || forceReinitialize)
    //    //        {
    //    //            var fileIOManagerInitResult = await this.fileIOManager.InitializeAsync().ConfigureAwait(false);
    //    //            if (!fileIOManagerInitResult.Success) return CallResult.BuildFailedCallResult(fileIOManagerInitResult, "Failed to initialize fileIOManager: {0}");
    //    //        }

    //    //        this.fileIOManagerWrapper = new FileIOManager.FileIOManager.Wrapper(this.fileIOManager);
    //    //        this.paths = this.proxySettings.FileStructure.BuildPaths(this.fileIOManagerWrapper);

    //    //        this.isInitialized = true;

    //    //        return new CallResult();
    //    //    }
    //    //    catch(Exception ex) { return CallResult.FromException(ex); }
    //    //}

    //    public async Task<ICallResult<IUpdaterDefinition>> RetrieveUpdaterDefinitionAsync(Guid updaterDefinitionId)
    //    {
    //        if (!this.isInitialized) return new CallResult<IUpdaterDefinition>(false, "Not initialized");

    //        try
    //        {
    //            var updaterDefinitionFilePath = this.fileIOManagerWrapper.BuildAppendedPath(this.paths.UpdaterDefinitionsFolder.Path, updaterDefinitionId.ToString());

    //            var createReadStreamResult = this.fileIOManager.CreateReadFileStream(updaterDefinitionFilePath);
    //            if (!createReadStreamResult.Success) return BuildFailedToReadUpdaterDefinitionCallResult(updaterDefinitionId, createReadStreamResult.ErrorText);

    //            string updaterDefinitionJson;
    //            using (var sr = new StreamReader(createReadStreamResult.Result)) updaterDefinitionJson = await sr.ReadToEndAsync().ConfigureAwait(false);

    //            if (string.IsNullOrEmpty(updaterDefinitionJson)) return BuildFailedToReadUpdaterDefinitionCallResult(updaterDefinitionId);

    //            var updaterDefinitionModel = JsonSerializer.Deserialize<UpdaterDefinitionModel>(updaterDefinitionJson); if (updaterDefinitionModel == null) return BuildFailedToReadUpdaterDefinitionCallResult(updaterDefinitionId);
    //            var updaterDefinition = ModelTranslator.ToUpdaterDefinition(updaterDefinitionModel);

    //            return new CallResult<IUpdaterDefinition>(updaterDefinition);
    //        }
    //        catch (Exception ex) { return CallResult<IUpdaterDefinition>.FromException(ex); }
    //    }

    //    public ICallResult<Stream> CreateModuleReadStream(IUpdaterModuleDescription moduleDescription)
    //    {
    //        if(!this.isInitialized) return new CallResult<Stream>(false, "Not initialized.");

    //        try
    //        {
    //            var modulePath = this.fileIOManagerWrapper.BuildAppendedPath(this.paths.DllsFolder.Path, moduleDescription.AssemblyName);

    //            return this.fileIOManager.CreateReadFileStream(modulePath);

    //        } catch (Exception ex) { return CallResult<Stream>.FromException(ex); }
    //    }

    //    private static ICallResult<IUpdaterDefinition> BuildFailedToReadUpdaterDefinitionCallResult(Guid updaterDefinitionId, string errorText = "")
    //    {
    //        var errorMessage = $"Failed to read updater definition file for updater definition with Id: {updaterDefinitionId}";

    //        if (!string.IsNullOrEmpty(errorText)) errorMessage = $"{errorMessage}: {errorText}";

    //        return new CallResult<IUpdaterDefinition>(false, errorMessage);
    //    }
    //}
}

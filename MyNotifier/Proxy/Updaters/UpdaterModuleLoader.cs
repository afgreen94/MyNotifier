using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using MyNotifier.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Proxy.Updaters
{
    public interface IProxyIOManager
    {
        ICallResult<Stream> CreateModuleReadStream(IUpdaterModuleDescription moduleDescription);
        //string BuildModulePath(IUpdaterModuleDescription moduleDescription);
    }

    public interface IProxyUpdaterModuleLoader : IUpdaterModuleLoader { }
    public class UpdaterModuleLoader : MyNotifier.Updaters.UpdaterModuleLoader, IProxyUpdaterModuleLoader
    {

        private readonly IProxyIOManager proxyIOManager;

        public UpdaterModuleLoader(IProxyIOManager proxyIOManager, IConfiguration configuration, ICallContext<UpdaterModuleLoader> callContext) : base(configuration, callContext) { }

        protected override async Task<ICallResult<byte[]>> LoadModuleBytesAsync(IUpdaterDefinition updaterDefinition)
        {
            try
            {
                //var modulePath = this.proxyIOManager.BuildModulePath(updaterDefinition.ModuleDescription);
                //var createDllReadStreamResult = this.fileIOManager.CreateReadFileStream(modulePath);

                var createModuleStreamResult = proxyIOManager.CreateModuleReadStream(updaterDefinition.ModuleDescription);
                if (!createModuleStreamResult.Success) return CallResult<byte[]>.BuildFailedCallResult(createModuleStreamResult, $"Failed to create module read stream for updater: {updaterDefinition.Id}-{updaterDefinition.Name}: {0}");

                using var dllReadStream = createModuleStreamResult.Result;
                using var ms = new MemoryStream();

                await dllReadStream.CopyToAsync(ms).ConfigureAwait(false);

                var moduleBytes = ms.ToArray();     //moduleBytes = await this.LoadDllBytesFromPathAsync(modulePath).ConfigureAwait(false);

                return new CallResult<byte[]>(moduleBytes);
            }
            catch (Exception ex) { return CallResult<byte[]>.FromException(ex); }
        }

        public new interface IConfiguration : MyNotifier.Updaters.UpdaterModuleLoader.IConfiguration { }
        public new class Configuration : MyNotifier.Updaters.UpdaterModuleLoader.Configuration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }
    }
}

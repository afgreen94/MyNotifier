using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using MyNotifier.Contracts.Proxy.Updaters;
using MyNotifier.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Proxy.Updaters
{
    public class ModuleLoader : MyNotifier.Updaters.ModuleLoader, IModuleLoader
    {
        private readonly IIOManager ioManager;

        public ModuleLoader(IIOManager ioManager, IConfiguration configuration, ICallContext<ModuleLoader> callContext) : base(configuration, callContext) { this.ioManager = ioManager; }

        protected override async Task<ICallResult<byte[]>> LoadModuleBytesAsync(IUpdaterDefinition updaterDefinition)
        {
            try
            {
                var createModuleStreamResult = this.ioManager.CreateUpdaterModuleReadStream(updaterDefinition.ModuleDescription);
                if (!createModuleStreamResult.Success) return CallResult<byte[]>.BuildFailedCallResult(createModuleStreamResult, $"Failed to create module read stream for updater: {updaterDefinition.Id}-{updaterDefinition.Name}: {0}");

                using var dllReadStream = createModuleStreamResult.Result;
                using var ms = new MemoryStream();

                await dllReadStream.CopyToAsync(ms).ConfigureAwait(false);

                var moduleBytes = ms.ToArray();     //moduleBytes = await this.LoadDllBytesFromPathAsync(modulePath).ConfigureAwait(false);

                return new CallResult<byte[]>(moduleBytes);
            }
            catch (Exception ex) { return CallResult<byte[]>.FromException(ex); }
        }

        public new interface IConfiguration : MyNotifier.Updaters.ModuleLoader.IConfiguration { }
        public new class Configuration : MyNotifier.Updaters.ModuleLoader.Configuration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }
    }
}

using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;

namespace MyNotifier.Updaters
{



    public abstract class ModuleLoader : IModuleLoader
    {

        protected readonly IConfiguration configuration;
        protected readonly ICallContext<ModuleLoader> callContext;

        protected HashSet<Guid> loadedUpdaterIds = new();

        protected ModuleLoader(IConfiguration configuration, ICallContext<ModuleLoader> callContext)
        {
            this.configuration = configuration;
            this.callContext = callContext;
        }

        public async ValueTask<IModuleLoader.IResult> LoadModuleAsync(IUpdaterDefinition updaterDefinition) => await this.LoadModuleCoreAsync(updaterDefinition).ConfigureAwait(false);
        public async IAsyncEnumerable<IModuleLoader.IResult> LoadModulesAsync(params IUpdaterDefinition[] updaterDefinitions) { foreach (var updaterDefinition in updaterDefinitions) yield return await this.LoadModuleCoreAsync(updaterDefinition).ConfigureAwait(false); } //this may not work. something about try/catch in asyncEnumerables 

        protected virtual async ValueTask<IModuleLoader.IResult> LoadModuleCoreAsync(IUpdaterDefinition updaterDefinition)
        {
            try
            {
                if (this.loadedUpdaterIds.Contains(updaterDefinition.Id)) return new Result() { Success = true, Result = updaterDefinition };

                var moduleBytesResult = await this.LoadModuleBytesAsync(updaterDefinition).ConfigureAwait(false);

                if (!moduleBytesResult.Success) return new Result() { Success = false, ErrorText = moduleBytesResult.ErrorText };

                var assembly = Assembly.Load(moduleBytesResult.Result);

                if (!this.TryValidateAssemblyForUpdaterDefinition(assembly, updaterDefinition, out var errorText)) return new Result() { Success = false, ErrorText = errorText };

                this.loadedUpdaterIds.Add(updaterDefinition.Id);

                return new Result() { Success = true, Result = updaterDefinition };
            }
            catch (Exception ex) { return new Result() { Success = false, ErrorText = ex.Message }; }
        }

        protected virtual bool TryValidateAssemblyForUpdaterDefinition(Assembly assembly,
                                                                       IUpdaterDefinition updaterDefinition,
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
                    if (definedType.FullName == updaterDefinition.ModuleDescription.TypeFullName && Type.GetType($"{updaterDefinition.ModuleDescription.AssemblyName}.{updaterDefinition.ModuleDescription.TypeFullName}") != null) { updaterTypePresentAndRecognized = true; continue; }
                    if (definedType.FullName == updaterDefinition.ModuleDescription.DefinitionTypeFullName && Type.GetType($"{updaterDefinition.ModuleDescription.AssemblyName}.{updaterDefinition.ModuleDescription.DefinitionTypeFullName}") != null) { updaterDefinitionTypePresentAndRecognized = true; continue; }
                }

                if (!(updaterTypePresentAndRecognized && updaterDefinitionTypePresentAndRecognized))
                {
                    errorText = $"Could not find/recognize required types for updater: {updaterDefinition.Id}-{updaterDefinition.Name} in assembly: {assembly.FullName}";
                    return false;
                }

                errorText = string.Empty;
                return true;

            }
            catch (Exception ex) { errorText = ex.Message; return false; }
        }

        protected abstract Task<ICallResult<byte[]>> LoadModuleBytesAsync(IUpdaterDefinition updaterDefinition);

        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : ApplicationConfigurationWrapper
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }

        public class Result : CallResult<IUpdaterDefinition>, IModuleLoader.IResult { }
    }
}

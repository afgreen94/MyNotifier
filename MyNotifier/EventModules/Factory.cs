using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts.EventModules;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterValidator = MyNotifier.Parameters.ParameterValidator;
using MyNotifier.Updaters;
using IUpdaterFactory = MyNotifier.Contracts.Updaters.IFactory;
using IUpdaterDefinition = MyNotifier.Contracts.Updaters.IDefinition;
using IEventModuleDefinition = MyNotifier.Contracts.EventModules.IDefinition;
using CustomEventModuleDefinition = MyNotifier.Contracts.EventModules.CustomDefinition;
using IEventModuleParameterValues = MyNotifier.Contracts.EventModules.IParameterValues;

namespace MyNotifier.EventModules
{
    public class Factory : MyNotifier.Contracts.EventModules.IFactory
    {

        private readonly IProvider provider;
        private readonly IUpdaterFactory updaterFactory;
        private readonly Contracts.EventModules.ICache cache;
        private readonly IConfiguration configuration;
        private readonly ICallContext<Factory> callContext;

        private ParameterValidator parameterValidator = new();

        public Factory(IProvider provider,
                       IUpdaterFactory updaterFactory,
                       Contracts.EventModules.ICache cache,
                       IConfiguration configuration,
                       ICallContext<Factory> callContext)
        {
            this.provider = provider;
            this.updaterFactory = updaterFactory;
            this.cache = cache;
            this.configuration = configuration;
            this.callContext = callContext;
        }

        public IUpdaterFactory UpdaterFactory => this.updaterFactory;

        public async ValueTask<ICallResult<IEventModuleDefinition>> GetDefinitionAsync(Guid eventModuleDefinitionId)  //this & getAsync(Guid) can be abstracted 
        {
            try
            {
                if (this.cache.TryGetValue(eventModuleDefinitionId, out IEventModuleDefinition definition)) return new CallResult<IEventModuleDefinition>(definition);

                var getDefinitionResult = await this.provider.GetDefinitionAsync(eventModuleDefinitionId).ConfigureAwait(false);  //may not exist! 
                if (!getDefinitionResult.Success) return CallResult<IEventModuleDefinition>.BuildFailedCallResult(getDefinitionResult, $"Failed to produce event module definition with Id: {eventModuleDefinitionId}: {{0}}");

                this.cache.Add(definition.Id, definition);

                return new CallResult<IEventModuleDefinition>(definition);
            }
            catch (Exception ex) { return CallResult<IEventModuleDefinition>.FromException(ex); }

        }
        //event module instance id 
        public async ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleId) 
        {
            try
            {
                if (this.cache.TryGetValue(eventModuleId, out IEventModule eventModule)) return new CallResult<IEventModule>(eventModule);

                var getEventModuleResult = await this.provider.GetAsync(eventModuleId).ConfigureAwait(false);
                if (!getEventModuleResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getEventModuleResult, $"Failed to produce event module with Id: {eventModuleId}: {{0}}");

                this.cache.Add(eventModuleId, eventModule);

                return new CallResult<IEventModule>(eventModule);                

            } catch(Exception ex) { return CallResult<IEventModule>.FromException(ex); }
        }
        public async ValueTask<ICallResult<IEventModule>> GetAsync(EventModuleModel model)  //persist custom event module definitions?
        {
            try
            {
                var updaterDefinitions = new IUpdaterDefinition[model.Definition.UpdaterDefinitions.Length];

                for (int i = 0; i < updaterDefinitions.Length; i++) updaterDefinitions[i] = ModelTranslator.ToUpdaterDefinition(model.Definition.UpdaterDefinitions[i]);

                var eventModule = new CustomEventModule()
                {
                    Definition = new CustomEventModuleDefinition()
                    {
                        Id = model.Definition.Id,
                        Name = model.Definition.Name,
                        Description = model.Definition.Description,
                        UpdaterDefinitions = updaterDefinitions
                    }
                };

                //this is pretty common logic to the function above, should encapsulate 
                foreach (var updaterDefinition in eventModule.Definition.UpdaterDefinitions)
                {
                    //can parallelize getValidation of parameters and getUpdater
                    bool validParameters = this.parameterValidator.TryValidateParameters(updaterDefinition.ParameterDefinitions, model.Parameters[updaterDefinition.Id], out var errorText);
                    if (!validParameters) return new CallResult<IEventModule>(false, $"Invalid parameters for event module with Id: {eventModule.Definition.Id} and updater definition Id: {updaterDefinition.Id}: {errorText}");

                    var getUpdaterResult = await this.updaterFactory.GetAsync(updaterDefinition.Id).ConfigureAwait(false);
                    if (!getUpdaterResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getUpdaterResult, $"Failed to produce updater with definition Id: {updaterDefinition.Id} for event module with definition Id: {eventModule.Definition.Id}: {{0}}");

                    eventModule.UpdaterParameterWrappers.Add(updaterDefinition.Id, new UpdaterParametersWrapper()
                    {
                        Updater = getUpdaterResult.Result,
                        Parameters = model.Parameters[updaterDefinition.Id]
                    });
                }


                return new CallResult<IEventModule>(eventModule);
            }
            catch (Exception ex) { return CallResult<IEventModule>.FromException(ex); }

        }
        public ValueTask<ICallResult<IEventModule>> GetAsync(IEventModuleDefinition definition, IEventModuleParameterValues parameterValues) => throw new NotImplementedException();
        public async ValueTask<ICallResult<IEventModule>> GetAsync(Guid eventModuleDefinitionId, IEventModuleParameterValues parameterValues) //ParameterValue[][] parameterValues)
        {
            try
            {
                //could have cache of eventModules ??

                if (!this.cache.TryGetValue(eventModuleDefinitionId, out IEventModuleDefinition eventModuleDefinition))
                {
                    var getEventModuleDefinitionResult = await this.provider.GetDefinitionAsync(eventModuleDefinitionId);
                    if (!getEventModuleDefinitionResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getEventModuleDefinitionResult, $"Failed to produce event module definition with Id: {eventModuleDefinitionId}: {{0}}");

                    eventModuleDefinition = getEventModuleDefinitionResult.Result;

                    this.cache.Add(eventModuleDefinition.Id, eventModuleDefinition);
                }

                var eventModule = new CustomEventModule() { Definition = eventModuleDefinition };

                foreach (var updaterDefinition in eventModule.Definition.UpdaterDefinitions)
                {
                    //can parallelize getValidation of parameters and getUpdater
                    var validParameters = this.parameterValidator.TryValidateAndBuildParameters(updaterDefinition.ParameterDefinitions, parameterValues.UpdaterParameters[updaterDefinition.Id], out var parameters, out var errorText);
                    if (!validParameters) return new CallResult<IEventModule>(false, $"Invalid parameters for event module with Id: {eventModuleDefinitionId} and updater definition Id: {updaterDefinition.Id}: {errorText}");

                    var getUpdaterResult = await this.updaterFactory.GetAsync(updaterDefinition.Id).ConfigureAwait(false);
                    if (!getUpdaterResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getUpdaterResult, $"Failed to produce updater with definition Id: {updaterDefinition.Id} for event module with definition Id: {eventModuleDefinitionId}: {{0}}"); 

                    eventModule.UpdaterParameterWrappers.Add(updaterDefinition.Id, new UpdaterParametersWrapper()
                    {
                        Updater = getUpdaterResult.Result,
                        Parameters = parameters
                    });
                }

                //could cache eventModules by hash or something 

                return new CallResult<IEventModule>(eventModule);
            }
            catch (Exception ex) { return CallResult<IEventModule>.FromException(ex); }//return new CallResult(); } //handle 

        }
        public ValueTask<ICallResult<IEventModule>> GetAsync(string eventModuleString) => throw new NotImplementedException(); //event Module string representation/hash/json 

        //common getUpdater and validateParameters logic 
        //private async ValueTask PopulateUpdaterParameterWrappersAsync(EventModule eventModule, Func<IUpdaterDefinition, Parameter[]> getValidParameters)
        //{
        //    foreach (var updaterDefinition in eventModule.Definition.UpdaterDefinitions)
        //    {
        //        //can parallelize getValidation of parameters and getUpdater
        //        var parameters = getValidParameters(updaterDefinition);

        //        var getUpdaterResult = await this.updaterFactory.GetUpdaterAsync(updaterDefinition.Id).ConfigureAwait(false);
        //        if(!getUpdaterResult.Success) //handle 

        //        eventModule.UpdaterParameterWrappers.Add(updaterDefinition.Id, new UpdaterParametersWrapper()
        //        {
        //            Updater = updater,
        //            Parameters = parameters
        //        });
        //    }
        //}

        public interface IConfiguration : IApplicationConfigurationWrapper { }
        public class Configuration : ApplicationConfigurationWrapper, IConfiguration
        {
            public Configuration(IApplicationConfiguration innerApplicationConfiguration) : base(innerApplicationConfiguration)
            {
            }
        }
    }

}

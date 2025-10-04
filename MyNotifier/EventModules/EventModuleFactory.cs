using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterValidator = MyNotifier.Parameters.ParameterValidator;
using MyNotifier.Updaters;
using MyNotifier.Contracts.EventModules;

namespace MyNotifier.EventModules
{
    public class EventModuleFactory : IEventModuleFactory
    {

        private readonly IEventModuleProvider provider;
        private readonly IUpdaterFactory updaterFactory;
        private readonly Contracts.EventModules.ICache cache;
        private readonly IConfiguration configuration;
        private readonly ICallContext<EventModuleFactory> callContext;

        private ParameterValidator parameterValidator = new();

        public EventModuleFactory(IEventModuleProvider provider,
                                  IUpdaterFactory updaterFactory,
                                  Contracts.EventModules.ICache cache,
                                  IConfiguration configuration,
                                  ICallContext<EventModuleFactory> callContext)
        {
            this.provider = provider;
            this.updaterFactory = updaterFactory;
            this.cache = cache;
            this.configuration = configuration;
            this.callContext = callContext;
        }


        public IUpdaterFactory UpdaterFactory => this.updaterFactory;

        public async ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleDefinitionId, IEventModuleParameterValues parameterValues) //ParameterValue[][] parameterValues)
        {
            try
            {

                //could have cache of eventModules ??

                if (!this.cache.TryGetValue(eventModuleDefinitionId, out IEventModuleDefinition eventModuleDefinition))
                {
                    var getEventModuleDefinitionResult = await this.provider.GetEventModuleDefinitionAsync(eventModuleDefinitionId);
                    if (!getEventModuleDefinitionResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getEventModuleDefinitionResult, $"Failed to retrieve event module definition with id: {eventModuleDefinitionId}: {{0}}");

                    eventModuleDefinition = getEventModuleDefinitionResult.Result;

                    this.cache.Add(eventModuleDefinition.Id, eventModuleDefinition);
                }

                var eventModule = new EventModule() { Definition = eventModuleDefinition };

                foreach (var updaterDefinition in eventModule.Definition.UpdaterDefinitions)
                {
                    //can parallelize getValidation of parameters and getUpdater
                    var validParameters = this.parameterValidator.TryValidateAndBuildParameters(updaterDefinition.ParameterDefinitions, parameterValues.UpdaterParameters[updaterDefinition.Id], out var parameters, out var errorText);
                    if (!validParameters) return new CallResult<IEventModule>(false, $"Invalid parameters for event module with id: {eventModuleDefinitionId} and updater definition id: {updaterDefinition.Id}: {errorText}");

                    var getUpdaterResult = await this.updaterFactory.GetUpdaterAsync(updaterDefinition.Id).ConfigureAwait(false);
                    if (!getUpdaterResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getUpdaterResult, $"Failed to get updater with definition id: {updaterDefinition.Id} for event module with definition id: {eventModuleDefinitionId}: {{0}}"); 

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
        public async ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(EventModuleModel model)  //persist custom event module definitions?
        {
            try
            {
                var updaterDefinitions = new IUpdaterDefinition[model.Definition.UpdaterDefinitions.Length];

                for (int i = 0; i < updaterDefinitions.Length; i++) updaterDefinitions[i] = ModelTranslator.ToUpdaterDefinition(model.Definition.UpdaterDefinitions[i]);

                var eventModule = new EventModule()
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
                    if(!validParameters) return new CallResult<IEventModule>(false, $"Invalid parameters for event module with id: {eventModule.Definition.Id} and updater definition id: {updaterDefinition.Id}: {errorText}");

                    var getUpdaterResult = await this.updaterFactory.GetUpdaterAsync(updaterDefinition.Id).ConfigureAwait(false);
                    if (!getUpdaterResult.Success) return CallResult<IEventModule>.BuildFailedCallResult(getUpdaterResult, $"Failed to get updater with definition id: {updaterDefinition.Id} for event module with definition id: {eventModule.Definition.Id}: {{0}}");

                    eventModule.UpdaterParameterWrappers.Add(updaterDefinition.Id, new UpdaterParametersWrapper()
                    {
                        Updater = getUpdaterResult.Result,
                        Parameters = model.Parameters[updaterDefinition.Id]
                    });
                }


                return new CallResult<IEventModule>(eventModule);
            }
            catch(Exception ex) { return CallResult<IEventModule>.FromException(ex); }

        }

        public ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(Guid eventModuleId) => throw new NotImplementedException();//event module instance id 
        public ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(string eventModuleString) => throw new NotImplementedException(); //event Module string representation/hash/json 
        public ValueTask<ICallResult<IEventModule>> GetEventModuleAsync(IEventModuleDefinition definition, IEventModuleParameterValues parameterValues) => throw new NotImplementedException();

        public async ValueTask<ICallResult<IEventModuleDefinition>> GetEventModuleDefinitionAsync(Guid eventModuleDefinitionId)
        {
            try
            {
                if (this.cache.TryGetValue(eventModuleDefinitionId, out IEventModuleDefinition definition)) return new CallResult<IEventModuleDefinition>(definition);

                var getDefinitionResult = await this.provider.GetEventModuleDefinitionAsync(eventModuleDefinitionId).ConfigureAwait(false);  //may not exist! 
                if (!getDefinitionResult.Success) return CallResult<IEventModuleDefinition>.BuildFailedCallResult(getDefinitionResult, $"Failed to retrieve event module definition with id: {eventModuleDefinitionId}: {{0}}");

                this.cache.Add(definition.Id, definition);

                return new CallResult<IEventModuleDefinition>(definition);
            }
            catch (Exception ex) { return CallResult<IEventModuleDefinition>.FromException(ex); }

        }

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

using MyNotifier.Contracts.Updaters;
using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNotifier.Parameters;

namespace MyNotifier
{
    //public class EventModuleHelper
    //{
    //    public IEventModule BuildEventModule() => throw new NotImplementedException();
    //    public bool TryBuildEventModule(IEventModuleDefinition definition,
    //                                    string description,
    //                                    IDictionary<Guid, ParameterValue[]> parameterValuesMap,
    //                                    IUpdaterFactory updaterFactory,
    //                                    out IEventModule eventModule,
    //                                    out string errorText)
    //    {
    //        try
    //        {
    //            eventModule = null;
    //            errorText = string.Empty;

    //            var parameterValidator = new ParameterValidator();

    //            var updatersMap = new Dictionary<Guid, IUpdater>();
    //            var parametersMap = new Dictionary<Guid, Parameter[]>();

    //            foreach (var updaterDefinition in definition.UpdaterDefinitions)
    //            {
    //                var parameterDefinitions = updaterDefinition.ParameterDefinitions;
    //                var parameterValues = parameterValuesMap[updaterDefinition.Id];

    //                var validated = parameterValidator.TryValidateAndBuildParameters(parameterDefinitions, parameterValues, out var parameters, out var parameterValidatorErrorText);

    //                if (!validated) { errorText = parameterValidatorErrorText; return false; }

    //                if (!updatersMap.ContainsKey(updaterDefinition.Id)) updatersMap.Add(updaterDefinition.Id, updaterFactory.GetUpdater(updaterDefinition.Id));
    //                parametersMap[updaterDefinition.Id] = parameters;
    //            }

    //            eventModule = new EventModule()
    //            {
    //                Model = new EventModuleModel()
    //                {
    //                    Definition = definition,
    //                    Description = description
    //                },
    //                Updaters = [.. updatersMap.Values],
    //                Parameters = parametersMap
    //            };

    //            return true;
    //        } catch(Exception ex) { errorText = ex.Message; return false; }

    //    }
    //}
}

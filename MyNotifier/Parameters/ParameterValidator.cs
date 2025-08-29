using MyNotifier.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Parameters
{
    public class ParameterValidator 
    {
        public Parameter[] ValidateBuildParameters(IParameterDefinition[] parameterDefinitions, ParameterValue[] parameterValues) => throw new NotImplementedException();
        public void ValidateParameters(IParameterDefinition[] parameterDefinitions, Parameter[] parameters) => throw new NotImplementedException();
        public bool TryValidateParameters(IParameterDefinition[] parameterDefinitions, Parameter[] parameters, out string errorText) => throw new NotImplementedException();
        public bool TryValidateAndBuildParameters(IParameterDefinition[] parameterDefinitions, ParameterValue[] parameterValues, out Parameter[] parameters, out string errorText)
        {
            parameters = [];
            errorText = string.Empty;

            var parametersList = new List<Parameter>();

            try
            {
                foreach(var parameterDefinition in parameterDefinitions)
                {
                    var parameterFound = false;

                    foreach(var parameterValue in parameterValues)
                    {
                        var parameter = new Parameter() { Definition = parameterDefinition, Value = parameterValue };

                        errorText = $"Parameter value not found for parameter Definition: {parameterDefinition.Id}_{parameterDefinition.Name}.";

                        if (parameterValue.ParameterDefinitionId == parameterDefinition.Id)
                        {
                            parameterFound = true;
                            errorText = "Parameter value - parameter definition type mismatch.";

                            switch (parameterDefinition.Type)
                            {
                                case ParameterType.String:
                                    if (parameterValue.Value is not string) return false;
                                    break;
                                case ParameterType.Char:
                                    if (parameterValue.Value is not char) return false;
                                    break;
                                case ParameterType.Int:
                                    if (parameterValue.Value is not int) return false;
                                    break;
                                case ParameterType.Long:
                                    if (parameterValue.Value is not long) return false;
                                    break;
                                case ParameterType.Double:
                                    if (parameterValue.Value is not double) return false;
                                    break;
                                case ParameterType.Bool:
                                    if (parameterValue.Value is not bool) return false;
                                    break;
                                case ParameterType.Byte:
                                    if (parameterValue.Value is not byte) return false;
                                    break;
                            }
                        }

                        if (parameterDefinition.IsRequired && !parameterFound) return false;

                        parametersList.Add(parameter);
                        break;
                    }
                }

                parameters = [.. parametersList];

                return true;

            } catch(Exception ex) { errorText = ex.Message; return false; }
        }


        private static IDictionary<Guid, ParameterDefinition> BuildParameterDefinitionsMap(ParameterDefinition[] parameterDefinitions)
        {
            var ret = new Dictionary<Guid, ParameterDefinition>();

            foreach(var parameterDefinition in parameterDefinitions) ret.Add(parameterDefinition.Id, parameterDefinition);

            return ret;
        }
    }
}

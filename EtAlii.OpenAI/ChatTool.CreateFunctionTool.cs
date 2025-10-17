#nullable disable

using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using OpenAI.Chat;

namespace EtAlii.OpenAI;

public static class ChatToolEx
{
    // As long as function tools are not made dynamic then not too much mappings are needed so let's keep a local dictionary. 
    private static readonly ConditionalWeakTable<ChatTool, ChatToolExtensionProperties> Mappings = new();

    // extension(ChatTool target)
    // {
    //      public FunctionToolInvocationInfo Invocation { get => Mappings.GetOrCreateValue(target)!.Invocation; set => Mappings.GetOrCreateValue(target)!.Invocation = value; }

    public static bool TryGetInvocationInfo(ChatToolCall toolCall, [CanBeNull] out FunctionToolInvocationInfo invocationInfo)
    {
        var mapping = Mappings.SingleOrDefault(m => m.Key.FunctionName == toolCall.FunctionName);
        if (mapping.Value is not null)
        {
            invocationInfo = mapping.Value.Invocation;
            return true;
        }
        invocationInfo = null!;
        return false;
    }

    public static FunctionToolInvocationInfo GetInvocation(this ChatTool tool) => Mappings.GetOrCreateValue(tool)!.Invocation;
    
    public static void SetInvocation(this ChatTool tool, FunctionToolInvocationInfo invocationInfo) => Mappings.GetOrCreateValue(tool)!.Invocation = invocationInfo;

    public static ChatTool CreateFunctionTool<TParam1, TParam2, TResult>(Func<TParam1, TParam2, TResult> function)
    {
        if (function is null)
        {
            throw new ArgumentException("No function provided", nameof(function));
        }
        
        var method = function.Method;
        
        var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>(false);
        if (descriptionAttribute is null)
        {
            throw new ArgumentException($"A description attribute should be provided for method {method.Name} to be registered as a tool.", nameof(function));
        }
        var functionDescription = descriptionAttribute.Description;

        var parameters = method.GetParameters();

        var parameterMappings = new List<(string Name, string Type, string Description, ParameterInfo Parameter)>();
        foreach (var parameter in parameters)
        {
            if (parameter.IsOptional)
            {
                throw new ArgumentException($"Parameter {parameter.Name} on method {method.Name} is optional. This is not supported yet for the method to be registered as a tool.", nameof(function));
            }
            if(parameter.HasDefaultValue)
            {
                throw new ArgumentException($"Parameter {parameter.Name} on method {method.Name} has a default value. This is not supported yet for the method to be registered as a tool.", nameof(function));
            }
            descriptionAttribute = parameter.GetCustomAttribute<DescriptionAttribute>(false);
            if (descriptionAttribute is null)
            {
                throw new ArgumentException($"A description attribute should be provided for method {method.Name} to be registered as a tool.", nameof(function));
            }

            var jsonType = JsonParameter.GetJsonType(parameter, method);

            parameterMappings.Add((parameter.Name, jsonType, descriptionAttribute.Description, Parameter: parameter));
        }

        var functionParameters = BinaryData.FromObjectAsJson(new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = parameterMappings.ToDictionary(kvp => kvp.Name, ToPropertyDefinition),
            ["required"] = parameters.Select(p => p.Name).ToArray(),
            ["additionalProperties"] = false,
        });
        
        var ct = ChatTool.CreateFunctionTool(method.Name, functionDescription, functionParameters, true);

        ct.SetInvocation(new FunctionToolInvocationInfo
        {
            Instance = null,
            Method = method,
            Parameters = parameters,
        });

        return ct;
    }

    private static object ToPropertyDefinition((string Name, string Type, string Description, ParameterInfo parameter) mapping)
    {
        switch (mapping.parameter.ParameterType)
        {
            case var p when p.IsEnum:
            {
                var enumValues = mapping.parameter.ParameterType.GetEnumValues().Cast<string>().ToArray();
                return new
                {
                    type = mapping.Type,
                    @enum = enumValues,
                    description = mapping.Description
                };
            }
            default:
                return new
                {
                    type = mapping.Type,
                    description = mapping.Description
                };
        }
    }
}
//}
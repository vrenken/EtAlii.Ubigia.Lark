#nullable disable

using System.Text.Json;
using OpenAI.Chat;

namespace EtAlii.OpenAI;

public static class ChatToolCallInvokeOnExtension
{
    /// <summary>
    /// Invokes a specific method according for a ChatTool call. For this the specified method defined in the <see cref="FunctionToolInvocationInfo"/> is raised with the provided parameters
    /// extracted from the <see cref="ChatToolCall"/>'s function arguments. The result is returned as a <see cref="ToolChatMessage"/>. This message either contains a string
    /// result or a serialized instance. The idea is that this method allows for very fast ChatTool to Function mappings. 
    /// </summary>
    /// <param name="toolCall">An instance of <see cref="ChatToolCall"/> containing the function arguments to be used for invocation.</param>
    /// <param name="info">Details of the method to be invoked, including its method info, parameters, and instance if applicable.</param>
    /// <returns>
    /// A <see cref="ToolChatMessage"/> representing the result of the invoked method, serialized if necessary.
    /// Throws specific exceptions if arguments are missing or if the method invocation fails.
    /// </returns>
    public static ToolChatMessage InvokeOn(this ChatToolCall toolCall, FunctionToolInvocationInfo info)
    {
        using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

        var parameters = new List<object>();

        foreach (var parameter in info.Parameters)
        {
            var hasParameter = argumentsJson.RootElement.TryGetProperty(parameter.Name!, out var parameterJsonElement);
            if (!hasParameter)
            {
                throw new ArgumentNullException(nameof(parameter.Name), $"The {parameter.Name} argument is not provided in the function call.");
            }

            var parameterValue = JsonParameter.GetValue(parameterJsonElement, parameter);
            parameters.Add(parameterValue);
        }
        var toolResult = info.Method.Invoke(info.Instance, parameters.ToArray());

        switch (toolResult)
        {
            case string stringToolResult:
                return new ToolChatMessage(toolCall.Id, stringToolResult);
            case not null:
                var response = JsonSerializer.Serialize(toolResult);
                return new ToolChatMessage(toolCall.Id, response);
            default:
                throw new InvalidOperationException($"The method {info.Method.Name} did not return a result.");
        }
    }
}

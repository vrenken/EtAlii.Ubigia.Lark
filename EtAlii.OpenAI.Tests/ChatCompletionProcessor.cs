using System.Text.Json;
using OpenAI.Chat;

namespace EtAlii.OpenAI.Tests;

public partial class TestChatCompletionProcessor : ChatCompletionProcessor
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TestChatCompletionProcessor(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public override async Task<IReadOnlyList<ChatMessage>> Process(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options)
    {
        var history = await base.Process(client, messages, options);

        foreach (var message in history)
        {
            switch (message)
            {
                case UserChatMessage userMessage:
                    _testOutputHelper.WriteLine("[USER]:");
                    _testOutputHelper.WriteLine(userMessage.Content[0].Text);
                    _testOutputHelper.WriteLine("");
                    break;

                case AssistantChatMessage assistantMessage when assistantMessage.Content.Count > 0:
                    _testOutputHelper.WriteLine("[ASSISTANT]:");
                    _testOutputHelper.WriteLine(assistantMessage.Content[0].Text);
                    _testOutputHelper.WriteLine("");
                    break;

                case ToolChatMessage:
                    // Do not print any tool messages; let the assistant summarize the tool results instead.
                    break;
            }
        }
        return history;
    }

    protected override void HandleToolCall(ChatToolCall toolCall, IReadOnlyList<ChatMessage> history)
    {
        switch (toolCall.FunctionName)
        {
            case nameof(GetCurrentLocation):
                {
                    var toolResult = GetCurrentLocation();
                    AddMessage(new ToolChatMessage(toolCall.Id, toolResult), history);
                    break;
                }

            case nameof(GetCurrentWeatherOld):
            {
                // The arguments that the model wants to use to call the function are specified as a
                // stringified JSON object based on the schema defined in the tool definition. Note that
                // the model may hallucinate arguments too. Consequently, it is important to do the
                // appropriate parsing and validation before calling the function.
                using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                var hasLocation = argumentsJson.RootElement.TryGetProperty("location", out var location);
                var hasUnit = argumentsJson.RootElement.TryGetProperty("unit", out var unit);

                if (!hasLocation)
                {
                    throw new ArgumentNullException(nameof(location), "The location argument is required.");
                }

                var toolResult = hasUnit
                    ? GetCurrentWeatherOld(location.GetString()!, unit.GetString()!)
                    : GetCurrentWeatherOld(location.GetString()!);
                AddMessage(new ToolChatMessage(toolCall.Id, toolResult), history);

                break;
            }

            case nameof(SidcTestTools.GetSidcRefinementOptionsManual):
            {
                using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                var hasSidc = argumentsJson.RootElement.TryGetProperty("sidc", out var sidc);
                var hasHint = argumentsJson.RootElement.TryGetProperty("hint", out var hint);

                if (!hasSidc)
                {
                    throw new ArgumentNullException(nameof(sidc), "The sidc argument is required.");
                }

                var toolResult = hasHint
                    ? SidcTestTools.GetSidcRefinementOptionsManual(sidc.GetString()!, hint.GetString()!)
                    : SidcTestTools.GetSidcRefinementOptionsManual(sidc.GetString()!, null!);
                var response = JsonSerializer.Serialize(toolResult);
                AddMessage(new ToolChatMessage(toolCall.Id, response), history);
                break;
            }

            default:
            {
                base.HandleToolCall(toolCall, history);
                break;
            }
        }
    }
}
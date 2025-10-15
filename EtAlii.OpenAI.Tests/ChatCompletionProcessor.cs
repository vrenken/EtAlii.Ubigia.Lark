using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using Xunit.Abstractions;

namespace EtAlii.OpenAI.Tests;

public partial class ChatCompletionProcessor
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ChatCompletionProcessor(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public async Task Process(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options)
    {
        var history = new List<ChatMessage>(messages);
        bool requiresAction;

        do
        {
            requiresAction = false;
            var contentBuilder = new StringBuilder();
            StreamingChatToolCallsBuilder toolCallsBuilder = new();

            var updatesStream = client.CompleteChatStreamingAsync(history, options);

            await foreach (var update in updatesStream)
            {
                // Accumulate the text content as new updates arrive.
                foreach (var contentPart in update.ContentUpdate)
                {
                    contentBuilder.Append(contentPart.Text);
                }

                // Build the tool calls as new updates arrive.
                foreach (var toolCallUpdate in update.ToolCallUpdates)
                {
                    toolCallsBuilder.Append(toolCallUpdate);
                }

                switch (update.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            // Add the assistant message to the conversation history.
                            history.Add(new AssistantChatMessage(contentBuilder.ToString()));
                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        {
                            // First, collect the accumulated function arguments into complete tool calls to be processed
                            var toolCalls = toolCallsBuilder.Build();

                            // Next, add the assistant message with tool calls to the conversation history.
                            AssistantChatMessage assistantMessage = new(toolCalls);

                            if (contentBuilder.Length > 0)
                            {
                                assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(contentBuilder.ToString()));
                            }
                            history.Add(assistantMessage);

                            // Then, add a new tool message for each tool call to be resolved.
                            foreach (var toolCall in toolCalls)
                            {
                                switch (toolCall.FunctionName)
                                {
                                    case nameof(GetCurrentLocation):
                                        {
                                            var toolResult = GetCurrentLocation();
                                            history.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    case nameof(GetCurrentWeather):
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
                                            ? GetCurrentWeather(location.GetString()!, unit.GetString()!)
                                            : GetCurrentWeather(location.GetString()!);
                                        history.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                        break;
                                    }

                                    case nameof(GetSidcRefinementOptions):
                                    {
                                        using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                        var hasSidc = argumentsJson.RootElement.TryGetProperty("sidc", out var sidc);
                                        var hasHint = argumentsJson.RootElement.TryGetProperty("hint", out var hint);

                                        if (!hasSidc)
                                        {
                                            throw new ArgumentNullException(nameof(sidc), "The sidc argument is required.");
                                        }

                                        var toolResult = hasHint
                                            ? GetSidcRefinementOptions(sidc.GetString()!, hint.GetString()!)
                                            : GetSidcRefinementOptions(sidc.GetString()!);
                                        var response = JsonSerializer.Serialize(toolResult);
                                        history.Add(new ToolChatMessage(toolCall.Id, response));
                                        break;
                                    }

                                    default:
                                        {
                                            // Handle other unexpected calls.
                                            throw new NotImplementedException();
                                        }
                                }
                            }

                            requiresAction = true;
                            break;
                        }

                    case ChatFinishReason.Length: throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");
                    case ChatFinishReason.ContentFilter: throw new NotImplementedException("Omitted content due to a content filter flag.");
                    case ChatFinishReason.FunctionCall: throw new NotImplementedException("Deprecated in favor of tool calls.");
                    default: throw new NotImplementedException(update.FinishReason.ToString());
                    case null:
                        break;
                }
            }
        } while (requiresAction);

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
    }
}
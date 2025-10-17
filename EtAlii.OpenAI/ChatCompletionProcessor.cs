using System.Text;
using OpenAI.Chat;

namespace EtAlii.OpenAI;

/// <summary>
/// Represents a processor for handling chat completions using an OpenAI ChatClient.
/// </summary>
public class ChatCompletionProcessor
{
    public event Action<ChatMessage>? Message;

    /// <summary>
    /// Processes a conversation using the provided OpenAI ChatClient, handles message streaming and calls tool methods accordingly.
    /// During the process the class raises update messages and afterwards  returns the final conversation history.
    /// </summary>
    /// <param name="client">The ChatClient instance used for processing the chat conversation.</param>
    /// <param name="messages">An array of initial chat messages to process and include in the conversation.</param>
    /// <param name="options">Configuration options for the chat completion, such as model settings and parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of chat messages that form the updated conversation history.</returns>
    public virtual async Task<IReadOnlyList<ChatMessage>> Process(ChatClient client, ChatMessage[] messages, ChatCompletionOptions options)
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
                        AddMessage(new AssistantChatMessage(contentBuilder.ToString()), history);
                        break;
                    }

                    case ChatFinishReason.ToolCalls:
                    {
                        // First, collect the accumulated function arguments into complete tool calls to be processed
                        var toolCalls = toolCallsBuilder.Build();

                        // Next, add the assistant message with tool calls to the conversation history.
                        var assistantMessage = new AssistantChatMessage(toolCalls);

                        if (contentBuilder.Length > 0)
                        {
                            assistantMessage.Content.Add(ChatMessageContentPart.CreateTextPart(contentBuilder.ToString()));
                        }
                        AddMessage(assistantMessage, history);

                        // Then, invoke functions for each toolcall and add a new tool message with the result as a response.
                        foreach (var toolCall in toolCalls)
                        {
                            HandleToolCall(toolCall, history);
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

        return history;
    }

    /// <summary>
    /// Adds a chat message to the conversation history and optionally invokes the Message event.
    /// </summary>
    /// <param name="message">The chat message to add to the conversation history.</param>
    /// <param name="history">The collection representing the chat history where the message should be added.</param>
    protected void AddMessage(ChatMessage message, IReadOnlyList<ChatMessage> history)
    {
        ((List<ChatMessage>)history).Add(message);
        Message?.Invoke(message);
    }

    /// <summary>
    /// Handles the invocation of a specific tool call and adds the resulting message to the conversation history.
    /// </summary>
    /// <param name="toolCall">The tool call instance to be processed.</param>
    /// <param name="history">The collection representing the chat history where the resulting message should be added.</param>
    protected virtual void HandleToolCall(ChatToolCall toolCall, IReadOnlyList<ChatMessage> history)
    {
        if (!ChatToolEx.TryGetInvocationInfo(toolCall, out var invocationInfo))
        {
            // Handle other unexpected calls.
            throw new NotImplementedException($"Unable to find a function to handle tool call {toolCall.FunctionName} with.");
        }
        
        var message = toolCall.InvokeOn(invocationInfo);
        AddMessage(message, history);
    }
}
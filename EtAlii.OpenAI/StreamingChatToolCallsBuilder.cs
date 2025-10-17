using System.Buffers;
using OpenAI.Chat;

namespace EtAlii.OpenAI;

/// <summary>
/// Provides functionality for progressively building tool calls from streaming chat updates.
/// </summary>
/// <remarks>
/// This class is used to process streaming updates of chat tool calls and accumulate the necessary
/// components, such as tool call IDs, function names, and function arguments, to construct a complete
/// list of tool calls.
/// </remarks>
public class StreamingChatToolCallsBuilder
    {
        private readonly Dictionary<int, string> _indexToToolCallId = [];
        private readonly Dictionary<int, string> _indexToFunctionName = [];
        private readonly Dictionary<int, SequenceBuilder<byte>> _indexToFunctionArguments = [];

        /// <summary>
        /// Appends a streaming chat tool call update to the builder for progressive accumulation of tool call data.
        /// </summary>
        /// <param name="toolCallUpdate">
        /// The streaming chat tool call update containing information such as the tool call ID, function name,
        /// and function arguments to be added to the builder.
        /// </param>
        public void Append(StreamingChatToolCallUpdate toolCallUpdate)
        {
            // Keep track of which tool call ID belongs to this update index.
            if (toolCallUpdate.ToolCallId != null)
            {
                _indexToToolCallId[toolCallUpdate.Index] = toolCallUpdate.ToolCallId;
            }

            // Keep track of which function name belongs to this update index.
            if (toolCallUpdate.FunctionName != null)
            {
                _indexToFunctionName[toolCallUpdate.Index] = toolCallUpdate.FunctionName;
            }

            // Keep track of which function arguments belong to this update index,
            // and accumulate the arguments as new updates arrive.
            if (toolCallUpdate.FunctionArgumentsUpdate != null && !toolCallUpdate.FunctionArgumentsUpdate.ToMemory().IsEmpty)
            {
                if (!_indexToFunctionArguments.TryGetValue(toolCallUpdate.Index, out var argumentsBuilder))
                {
                    argumentsBuilder = new SequenceBuilder<byte>();
                    _indexToFunctionArguments[toolCallUpdate.Index] = argumentsBuilder;
                }

                argumentsBuilder.Append(toolCallUpdate.FunctionArgumentsUpdate);
            }
        }

        /// <summary>
        /// Constructs a list of completed tool calls based on the accumulated tool call information.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="ChatToolCall"/> objects, each representing a constructed tool call.
        /// </returns>
        public IReadOnlyList<ChatToolCall> Build()
        {
            List<ChatToolCall> toolCalls = [];

            foreach (var (index, toolCallId) in _indexToToolCallId)
            {
                var sequence = _indexToFunctionArguments[index].Build();

                var toolCall = ChatToolCall.CreateFunctionToolCall(
                    id: toolCallId,
                    functionName: _indexToFunctionName[index],
                    functionArguments: BinaryData.FromBytes(sequence.ToArray()));

                toolCalls.Add(toolCall);
            }

            return toolCalls;
        }
    }
using System.Buffers;
using System.Diagnostics;

namespace EtAlii.OpenAI;

/// <summary>
/// Provides functionality to construct a sequence of data chunks and build a contiguous read-only sequence.
/// </summary>
/// <typeparam name="T">The type of elements stored in the sequence.</typeparam>
/// <remarks>
/// This class is designed to efficiently create and manage an ordered sequence of data chunks. It can
/// append chunks of data and construct a single read-only sequence for consumption. The internal logic
/// ensures memory-efficient handling of sequences.
/// </remarks>
public class SequenceBuilder<T>
{
    private Segment _first = null!;
    private Segment _last = null!;

    /// <summary>
    /// Appends a chunk of data to the sequence builder.
    /// </summary>
    /// <param name="data">
    /// The chunk of data to append. This data is stored as a read-only memory object
    /// and will be included in the resulting sequence.
    /// </param>
    public void Append(ReadOnlyMemory<T> data)
    {
        if (_first == null!)
        {
            Debug.Assert(_last == null);
            _first = new Segment(data);
            _last = _first;
        }
        else
        {
            _last = _last.Append(data);
        }
    }

    /// <summary>
    /// Builds a contiguous read-only sequence from the aggregated data chunks.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadOnlySequence{T}"/> containing the aggregated data chunks. If no data chunks have been appended,
    /// an empty sequence is returned.
    /// </returns>
    public ReadOnlySequence<T> Build()
    {
        if (_first == null!)
        {
            Debug.Assert(_last == null);
            return ReadOnlySequence<T>.Empty;
        }

        if (_first == _last)
        {
            Debug.Assert(_first.Next == null);
            return new ReadOnlySequence<T>(_first.Memory);
        }

        return new ReadOnlySequence<T>(_first, 0, _last, _last.Memory.Length);
    }

    /// <summary>
    /// Represents a segment within a read-only sequence, maintaining a portion of the memory and a reference to the next segment.
    /// </summary>
    /// <remarks>
    /// This class is used internally by the <see cref="SequenceBuilder{T}"/> to manage linked segments of memory.
    /// Each segment maintains its own memory payload and tracks its position within the overall sequence.
    /// The linking mechanism enables efficient traversal and aggregation of segments to construct contiguous sequences.
    /// </remarks>
    internal sealed class Segment : ReadOnlySequenceSegment<T>
    {
        public Segment(ReadOnlyMemory<T> items) : this(items, 0)
        {
        }

        private Segment(ReadOnlyMemory<T> items, long runningIndex)
        {
            Debug.Assert(runningIndex >= 0);
            Memory = items;
            RunningIndex = runningIndex;
        }

        public Segment Append(ReadOnlyMemory<T> items)
        {
            long runningIndex;
            checked { runningIndex = RunningIndex + Memory.Length; }
            Segment segment = new(items, runningIndex);
            Next = segment;
            return segment;
        }
    }
}
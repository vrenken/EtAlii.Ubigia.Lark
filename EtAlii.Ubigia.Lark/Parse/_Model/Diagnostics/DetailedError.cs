namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Technical error detail entry.
/// </summary>
public class DetailedError
{
    public required string Message { get; init; } = string.Empty;
    public required string Symbol { get; init; } = string.Empty;
    public required int Position { get; init; }
    public required int Line { get; init; }
    public required int Column { get; init; }
}
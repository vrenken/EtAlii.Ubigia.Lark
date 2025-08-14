namespace EtAlii.Ubigia.Lark;

public record RangeValue : Value
{
    public required string From { get; init; }
    public required string To { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => "\"{From}\"..\"{To}\"";
}
namespace EtAlii.Ubigia.Lark;

public record Alias
{
    public required Expansion Expansion {get; init; }
    public required string Rule { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Rule)
            ? $"{Expansion}"
            : $"{Expansion} -> {Rule}";
    }
}

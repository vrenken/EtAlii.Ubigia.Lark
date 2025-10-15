namespace EtAlii.Text.Lark._Old;

public record Alias
{
    public required Expansion Expansion {get; init; }
    public required string Name { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name)
            ? $"{Expansion}"
            : $"{Expansion} -> {Name}";
    }
}

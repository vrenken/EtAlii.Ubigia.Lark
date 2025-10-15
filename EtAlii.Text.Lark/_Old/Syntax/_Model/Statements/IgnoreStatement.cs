namespace EtAlii.Text.Lark._Old;

public record IgnoreStatement : Statement
{
    public required Alias[] Expansions { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"%ignore {string.Join(" | ", Expansions.Select(e => e.ToString()))}";
    }
}
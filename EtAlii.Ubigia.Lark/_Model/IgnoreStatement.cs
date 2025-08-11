namespace EtAlii.Ubigia.Lark;

public record IgnoreStatement : Statement
{
    public required Alias[] Expansions { get; init; }

    public override string ToString()
    {
        return $"%ignore {string.Join(" | ", Expansions.Select(e => e.ToString()))}";
    }
}
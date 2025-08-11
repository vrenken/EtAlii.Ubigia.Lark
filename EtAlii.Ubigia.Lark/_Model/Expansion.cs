namespace EtAlii.Ubigia.Lark;

public record Expansion
{
    public required Expression[] Expressions { get; init; }

    public override string ToString()
    {
        return string.Join(' ', Expressions.Select(e => e.ToString()));
    }
}

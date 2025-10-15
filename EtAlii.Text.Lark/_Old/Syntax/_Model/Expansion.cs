namespace EtAlii.Text.Lark._Old;

public record Expansion
{
    public required Expression[] Expressions { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Join(' ', Expressions.Select(e => e.ToString()));
    }
}

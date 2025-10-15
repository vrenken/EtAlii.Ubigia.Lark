namespace EtAlii.Text.Lark._Old;

public record DeclareStatement : Statement
{
    public required string[] Names { get; init; }

    public override string ToString() => $"%declare {string.Join(' ', Names)}";
}
namespace EtAlii.Text.Lark._Old;

public record TildeExpression : Expression
{
    public required Atom Atom { get; init; }
    public required int From { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Atom} ~ {From}";
    }
}
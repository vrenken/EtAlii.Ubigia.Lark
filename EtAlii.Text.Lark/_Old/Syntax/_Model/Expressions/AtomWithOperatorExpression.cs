namespace EtAlii.Text.Lark._Old;

public record AtomWithOperatorExpression : Expression
{
    public required Atom Atom { get; init; }
    public required string Operator { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Atom}{Operator}";
    }
}
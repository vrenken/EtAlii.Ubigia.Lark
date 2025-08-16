namespace EtAlii.Ubigia.Lark;

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
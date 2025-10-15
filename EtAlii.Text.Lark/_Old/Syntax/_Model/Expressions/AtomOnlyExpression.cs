namespace EtAlii.Text.Lark._Old;

public record AtomOnlyExpression : Expression
{
    public required Atom Atom { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => Atom.ToString();
}
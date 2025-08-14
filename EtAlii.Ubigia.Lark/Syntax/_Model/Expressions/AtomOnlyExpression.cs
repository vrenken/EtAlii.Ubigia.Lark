namespace EtAlii.Ubigia.Lark;

public record AtomOnlyExpression : Expression
{
    public required Atom Atom { get; init; }
    
    public override string ToString() => Atom.ToString();
}
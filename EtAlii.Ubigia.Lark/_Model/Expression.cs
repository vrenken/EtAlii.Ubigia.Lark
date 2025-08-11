namespace EtAlii.Ubigia.Lark;

public record Expression
{
    public required Atom Atom { get; init; }
    

    public override string ToString()
    {
        return Atom.ToString();
    }
}

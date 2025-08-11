namespace EtAlii.Ubigia.Lark;

public record ValueAtom : Atom
{
    public required Value Value { get; init; }

    public override string ToString()
    {
        return Value.ToString();
    }
}
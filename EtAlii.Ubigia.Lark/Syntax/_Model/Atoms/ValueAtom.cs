namespace EtAlii.Ubigia.Lark;

public record ValueAtom : Atom
{
    public required Value Value { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
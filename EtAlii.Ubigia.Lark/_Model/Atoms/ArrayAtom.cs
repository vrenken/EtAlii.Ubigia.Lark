namespace EtAlii.Ubigia.Lark;

public record ArrayAtom : Atom
{
    public required Alias[] Expansions { get; init; }

    public override string ToString()
    {
        return $"[{string.Join(" | ", Expansions.Select(e => e.ToString()))}]";
    }
}
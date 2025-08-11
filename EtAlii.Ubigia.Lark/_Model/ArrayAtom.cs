namespace EtAlii.Ubigia.Lark;

public record ArrayAtom : Atom
{
    public required Alias[] Expansions { get; init; }
}
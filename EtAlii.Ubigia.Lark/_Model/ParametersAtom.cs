namespace EtAlii.Ubigia.Lark;

public record ParametersAtom : Atom
{
    public required Alias[] Expansions { get; init; }
}
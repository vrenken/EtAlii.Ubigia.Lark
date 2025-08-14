namespace EtAlii.Ubigia.Lark;

public record TildeRangeExpression : Expression
{
    public required Atom Atom { get; init; }
    public required int From { get; init; }
    public required int To { get; init; }

    public override string ToString()
    {
        return $"{Atom} ~ {From}..{To}";
    }
}
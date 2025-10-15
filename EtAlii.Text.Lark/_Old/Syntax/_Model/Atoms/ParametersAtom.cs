namespace EtAlii.Text.Lark._Old;

public record ParametersAtom : Atom
{
    public required Alias[] Expansions { get; init; }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return $"({string.Join(" | ", Expansions.Select(e => e.ToString()))})";
    }
}
namespace EtAlii.Text.Lark._Old;

public record NamedValueList : Value
{
    public required string Name { get; init; }

    public required Value[] Values { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}{{{string.Join(", ", Values.Select(v => v.ToString()))}}}";
    }
}
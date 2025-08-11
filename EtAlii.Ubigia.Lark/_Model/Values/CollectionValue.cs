namespace EtAlii.Ubigia.Lark;

public record CollectionValue : Value
{
    public required string Name { get; init; }

    public required Value[] Values { get; init; }
}
namespace EtAlii.Ubigia.Lark;

public record NameValue : Value
{ 
    public required string Name { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => Name;
}
namespace EtAlii.Ubigia.Lark;

public record NameValue : Value
{ 
    public required string Name { get; init; }
    
    public override string ToString() => Name;
}
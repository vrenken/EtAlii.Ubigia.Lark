namespace EtAlii.Ubigia.Lark;

public record RegexValue : Value
{ 
    public required string Regex { get; init; }
    
    public override string ToString() => Regex;
}
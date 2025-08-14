namespace EtAlii.Ubigia.Lark;

public record RegexValue : Value
{ 
    public required string Regex { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => Regex;
}
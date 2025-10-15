namespace EtAlii.Text.Lark._Old;

public record RegexValue : Value
{ 
    public required string Regex { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => Regex;
}
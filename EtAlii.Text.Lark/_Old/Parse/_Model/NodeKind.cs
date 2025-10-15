namespace EtAlii.Text.Lark._Old;

/// <summary>
/// The kind of parse tree node.
/// </summary>
public enum NodeKind
{
    Rule,
    Token,
    Alias,
    Literal,
    Regex,
    Range,
    Group
}
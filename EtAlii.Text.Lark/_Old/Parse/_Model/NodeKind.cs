namespace EtAlii.Ubigia.Lark;

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
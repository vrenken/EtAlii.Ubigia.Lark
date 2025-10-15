namespace EtAlii.Text.Lark._Old;

/// <summary>
/// A parse tree node produced by the runtime parser.
/// Includes rich metadata to support a future transformer layer.
/// </summary>
public class ParseTreeNode
{
    public required string Name { get; init; }
    public required int Start { get; init; }
    public required int Length { get; init; }
    public required string Text { get; init; }

    /// <summary>
    /// Specific node kind (Rule, Token, Alias, Literal, Regex, Range, Group).
    /// </summary>
    public NodeKind Kind { get; init; } = NodeKind.Rule;

    /// <summary>
    /// The originating rule name, when applicable.
    /// </summary>
    public string? RuleName { get; init; }

    /// <summary>
    /// The originating token name, when applicable.
    /// </summary>
    public string? TokenName { get; init; }

    /// <summary>
    /// The alias name applied for this node (if present in syntax).
    /// </summary>
    public string? AliasName { get; init; }

    public List<ParseTreeNode> Children { get; } = new();

    public override string ToString() => $"{Name} [{Start}..{Start + Length}): \"{Text}\"";
}
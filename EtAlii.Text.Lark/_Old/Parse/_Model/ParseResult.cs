namespace EtAlii.Text.Lark._Old;

/// <summary>
/// Result of parsing an input using a EbnfSyntax.
/// Includes expansive diagnostics close to Lark's error reporting.
/// </summary>
public class ParseResult
{
    public required bool Success { get; init; }
    public required string Input { get; init; }
    public ParseTreeNode? Root { get; init; }

    /// <summary>
    /// High-level error messages (human-readable).
    /// </summary>
    public string[] Errors { get; init; } = [];

    /// <summary>
    /// Expansive diagnostics describing the farthest error, expected symbols, and call stack.
    /// </summary>
    public ParseDiagnostics Diagnostics { get; init; } = new();
}
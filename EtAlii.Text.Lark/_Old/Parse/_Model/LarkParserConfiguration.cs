namespace EtAlii.Text.Lark._Old;

/// <summary>
/// Configuration for the runtime parser, modeled after Lark's options.
/// </summary>
public class LarkParserConfiguration
{
    public ParserAlgorithm Algorithm { get; init; } = ParserAlgorithm.Earley;
    public LexingMode Lexing { get; init; } = LexingMode.Standard;

    /// <summary>
    /// Enable parameterized rules (rule templates) semantics.
    /// </summary>
    public bool EnableParameterizedRules { get; init; } = false;

    /// <summary>
    /// Which bracket delimiters are used to invoke parameterized rules.
    /// </summary>
    public ParameterBracketStyle ParamBrackets { get; init; } = ParameterBracketStyle.Both;

    /// <summary>
    /// Ambiguity handling (e.g., 'resolve', 'explicit', etc.).
    /// </summary>
    public string Ambiguity { get; init; } = "resolve";

    /// <summary>
    /// When true, collect ambiguity information; otherwise pick a single best parse.
    /// </summary>
    public bool CollectAllParses { get; init; } = false;

    /// <summary>
    /// Optional override for start rule.
    /// </summary>
    public string? StartRule { get; init; }
}

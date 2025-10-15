// TODO: Apologies, generated using an LLM, probably not the best approach.

namespace EtAlii.Text.Lark._Old;

/// <summary>
/// Facade for the Lark runtime parser with pluggable algorithms and options.
/// </summary>
public class LarkRuntimeParser
{
    private readonly EbnfSyntax _grammar;
    private readonly LarkParserConfiguration _configuration;
    private readonly IParseAlgorithm _parseAlgorithm;

    public LarkRuntimeParser(EbnfSyntax grammar, LarkParserConfiguration? config = null)
    {
        _grammar = grammar;
        _configuration = config ?? new LarkParserConfiguration();

        _parseAlgorithm = _configuration.Algorithm switch
        {
            ParserAlgorithm.Earley => new EarleyParseAlgorithm(_grammar, _configuration),
            ParserAlgorithm.Lalr => new LalrParseAlgorithm(_grammar, _configuration),
            _ => new EarleyParseAlgorithm(_grammar, _configuration)
        };
    }

    /// <summary>
    /// Parse the given input text using the selected algorithm and options.
    /// </summary>
    public ParseResult Parse(string input)
    {
        return _parseAlgorithm.Parse(input);
    }

    /// <summary>
    /// Parse with an optional start rule override (per-call), preserving API compatibility.
    /// </summary>
    public ParseResult Parse(string input, string? startRule)
    {
        if (string.IsNullOrWhiteSpace(startRule))
        {
            return _parseAlgorithm.Parse(input);
        }

        // Create a temporary config instance with the overridden start rule.
        var configuration = new LarkParserConfiguration
        {
            Algorithm = _configuration.Algorithm,
            Lexing = _configuration.Lexing,
            EnableParameterizedRules = _configuration.EnableParameterizedRules,
            ParamBrackets = _configuration.ParamBrackets,
            Ambiguity = _configuration.Ambiguity,
            CollectAllParses = _configuration.CollectAllParses,
            StartRule = startRule
        };

        IParseAlgorithm parseAlgorithm = configuration.Algorithm switch
        {
            ParserAlgorithm.Earley => new EarleyParseAlgorithm(_grammar, configuration),
            ParserAlgorithm.Lalr => new LalrParseAlgorithm(_grammar, configuration),
            _ => new EarleyParseAlgorithm(_grammar, configuration)
        };

        return parseAlgorithm.Parse(input);
    }
}

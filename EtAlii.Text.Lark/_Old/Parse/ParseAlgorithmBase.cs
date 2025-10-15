using System.Text.RegularExpressions;

// TODO: Apologies, generated using an LLM, probably not the best approach.

namespace EtAlii.Text.Lark._Old;

internal abstract partial class ParseAlgorithmBase
{
    protected readonly EbnfSyntax Syntax;
    protected readonly LarkParserConfiguration Configuration;

    protected readonly IReadOnlyDictionary<string, Rule> Rules;
    protected readonly IReadOnlyDictionary<string, Token> Tokens;
    protected readonly Expansion[] IgnoreExpansions;
    
    protected readonly Dictionary<string, Regex> RegexCache = new(StringComparer.Ordinal);

    protected readonly string AlgorithmName;

    // Parser state and diagnostics
    protected int FarthestPos;
    protected readonly HashSet<string> ExpectedAtFarthest = new(StringComparer.Ordinal);
    protected readonly Stack<string> CallStack = new();

    protected ParseAlgorithmBase(EbnfSyntax syntax, LarkParserConfiguration configuration, string algorithmName)
    {
        AlgorithmName = algorithmName;
        Syntax = syntax;
        Configuration = configuration;
        
        Rules = syntax.Rules.ToDictionary(r => r.Name, r => r, StringComparer.Ordinal);
        Tokens = syntax.Tokens.ToDictionary(t => t.Name, t => t, StringComparer.Ordinal);
        Rules = syntax.Rules.ToDictionary(r => r.Name, r => r, StringComparer.Ordinal);
        Tokens = syntax.Tokens.ToDictionary(t => t.Name, t => t, StringComparer.Ordinal);

        var ignoreExpansions = new List<Expansion>();
        foreach (var ig in syntax.Ignores)
        {
            foreach (var a in ig.Expansions)
            {
                ignoreExpansions.Add(a.Expansion);
            }
        }
        IgnoreExpansions = ignoreExpansions.ToArray();
    }
    
    protected bool TryResolveStartRule(string? startRuleName, out Rule? startRule)
    {
        if (!string.IsNullOrWhiteSpace(startRuleName) && Rules.TryGetValue(startRuleName, out startRule) || 
            Syntax is { HasStart: true, Start: not null } && Rules.TryGetValue(Syntax.Start.Name, out startRule))
        {
            return startRule != null!;
        }

        startRule = Syntax.Rules.FirstOrDefault();
        return startRule != null!;
    }
    
    protected void RegisterExpected(int pos, string symbol)
    {
        if (pos > FarthestPos)
        {
            FarthestPos = pos;
            ExpectedAtFarthest.Clear();
        }
        if (pos == FarthestPos)
        {
            ExpectedAtFarthest.Add(symbol);
        }
    }
}

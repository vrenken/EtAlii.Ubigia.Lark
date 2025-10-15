// TODO: Apologies, generated using an LLM, probably not the best approach.

using System.Text.RegularExpressions;

namespace EtAlii.Text.Lark._Old;

internal sealed class LalrParseAlgorithm : ParseAlgorithmBase, IParseAlgorithm
{
    // Scanner resources
    private readonly HashSet<string> _literals = new(StringComparer.Ordinal);
    
    public LalrParseAlgorithm(EbnfSyntax syntax, LarkParserConfiguration configuration)
        : base(syntax, configuration, "LALR(1)")
    {
        // Collect literal terminals from rules (e.g., "hello", " ")
        foreach (var rule in syntax.Rules)
        {
            foreach (var alias in rule.Expansions)
            {
                CollectLiterals(alias.Expansion);
            }
        }
    }

    public ParseResult Parse(string input)
    {
        FarthestPos = 0;
        ExpectedAtFarthest.Clear();
        CallStack.Clear();

        if (!TryResolveStartRule(Configuration.StartRule, out var startRule))
        {
            return new ParseResult
            {
                Success = false,
                Input = input,
                Errors = ["No start rule found in syntax."],
                Diagnostics = MakeDiagnostics(input, AlgorithmName, 0, [], [])
            };
        }

        // 1) Global tokenization (longest-match and token priority), with %ignore
        var (tokenStream, lexOk, lexErrorPos, lexExpected) = Tokenize(input);
        if (!lexOk)
        {
            var di = MakeDiagnostics(input, AlgorithmName, lexErrorPos, lexExpected.ToArray(), []);
            return new ParseResult
            {
                Success = false,
                Input = input,
                Errors = [FormatErrorMessage(input, di)],
                Diagnostics = di
            };
        }

        // 2) Deterministic parse over token stream
        var success = TryMatchRuleTokens(startRule!, input, tokenStream, 0, out var root, out var ti);
        var fullSuccess = success && ti == tokenStream.Count;

        var expected = ExpectedAtFarthest.ToArray();
        var diagnostics = MakeDiagnostics(input, AlgorithmName, FarthestPos, expected, CallStack.Reverse().ToArray());

        return new ParseResult
        {
            Success = fullSuccess,
            Input = input,
            Root = fullSuccess ? root : null,
            Errors = fullSuccess ? [] : [FormatErrorMessage(input, diagnostics)],
            Diagnostics = diagnostics
        };
    }

    // Token instance produced by the scanner
    private sealed record TokenInstance(string Name, string Text, int Start, int Length, bool IsLiteral);
    
    // ----------------------------
    //  Scanner (global lexing)
    // ----------------------------

    private (List<TokenInstance> tokens, bool ok, int errorPos, List<string> expected) Tokenize(string text)
    {
        var result = new List<TokenInstance>();
        var expected = new List<string>();
        var pos = 0;

        TokenInstance? bestToken = null;
        while (pos < text.Length)
        {
            // Skip %ignore
            if (TrySkipIgnored(text, pos, out var pAfterIgnore))
            {
                pos = pAfterIgnore;
                continue;
            }

            // Collect token candidates (from TOKEN rules)
            var bestPriority = int.MinValue;

            foreach (var t in Tokens.Values)
            {
                if (TryMatchTokenDefinition(t, text, pos, out var len))
                {
                    if (len > 0)
                    {
                        // Choose by priority first, then length
                        var priority = t.Priority;
                        if (priority > bestPriority || (priority == bestPriority && (bestToken == null || len > bestToken.Length)))
                        {
                            bestPriority = priority;
                            bestToken = new TokenInstance(t.Name, text.Substring(pos, len), pos, len, IsLiteral: false);
                        }
                    }
                }
            }

            // Collect literal candidates from rules
            TokenInstance? bestLiteral = null;
            foreach (var lit in _literals)
            {
                if (lit.Length == 0) continue;
                if (pos + lit.Length <= text.Length && text.AsSpan(pos, lit.Length).SequenceEqual(lit.AsSpan()))
                {
                    if (bestLiteral == null || lit.Length > bestLiteral.Length)
                    {
                        bestLiteral = new TokenInstance($"__LIT__:{lit}", lit, pos, lit.Length, IsLiteral: true);
                    }
                }
            }

            // Select overall best: token candidates vs literals by length (priority only among tokens)
            var chosen = 
                bestToken != null && bestLiteral != null
                ? bestToken.Length >= bestLiteral.Length ? bestToken : bestLiteral
                : bestToken ?? bestLiteral;

            if (chosen == null)
            {
                // No progress possible
                FarthestPos = Math.Max(FarthestPos, pos);
                expected.AddRange(Tokens.Keys);
                expected.AddRange(_literals.Select(l => $"\"{l}\""));
                return (result, false, pos, expected.Distinct().ToList());
            }

            result.Add(chosen);
            pos += chosen.Length;
        }

        return (result, true, pos, expected);
    }

    private bool TrySkipIgnored(string text, int pos, out int newPos)
    {
        var p = pos;
        var progressed = true;
        while (progressed)
        {
            progressed = false;
            foreach (var exp in IgnoreExpansions)
            {
                if (TryMatchExpansionText(exp, text, p, out var len))
                {
                    if (len > 0)
                    {
                        p += len;
                        progressed = true;
                        break;
                    }
                }
            }
        }
        newPos = p;
        return newPos > pos;
    }

    private bool TryMatchTokenDefinition(Token token, string text, int pos, out int length)
    {
        length = 0;
        var best = 0;
        foreach (var a in token.Expansions)
        {
            if (TryMatchExpansionText(a.Expansion, text, pos, out var len) && len > best)
            {
                best = len;
            }
        }
        length = best;
        return best > 0;
    }

    private bool TryMatchExpansionText(Expansion expansion, string text, int pos, out int length)
    {
        // A simplified textual match (token context) similar to the old engine
        length = 0;
        var p = pos;
        foreach (var expr in expansion.Expressions)
        {
            var (min, max) = GetRepetition(expr);
            var atom = GetAtom(expr);
            var count = 0;
            while (!max.HasValue || count < max.Value)
            {
                if (!TryMatchAtomText(atom, text, p, out var l))
                {
                    break;
                }
                p += l;
                count++;
                if (p >= text.Length) break;
            }

            if (count < min)
            {
                // On failure, the quantified expression must not consume input
                length = 0;
                return false;
            }
        }
        length = p - pos;
        // Reaching here means all expressions satisfied their minima
        return true;
    }

    private bool TryMatchAtomText(Atom atom, string text, int pos, out int length)
    {
        length = 0;
        switch (atom)
        {
            case ValueAtom va:
                return TryMatchValueText(va.Value, text, pos, out length);
            case ParametersAtom pa:
                foreach (var alt in pa.Expansions)
                {
                    if (TryMatchExpansionText(alt.Expansion, text, pos, out var l) && l > 0)
                    {
                        length = l;
                        return true;
                    }
                }
                return false;
            case ArrayAtom aa:
                foreach (var alt in aa.Expansions)
                {
                    if (TryMatchExpansionText(alt.Expansion, text, pos, out var l) && l > 0)
                    {
                        length = l;
                        return true;
                    }
                }
                return false;
            default:
                return false;
        }
    }

    private bool TryMatchValueText(Value value, string text, int pos, out int length)
    {
        length = 0;
        switch (value)
        {
            case NameValue nv:
                // Names are not used directly during token text matching, only literals/regex/ranges or nested collections
                if (IsQuoted(nv.Name))
                {
                    var lit = Unescape(Unquote(nv.Name));
                    if (pos + lit.Length <= text.Length && text.AsSpan(pos, lit.Length).SequenceEqual(lit.AsSpan()))
                    {
                        length = lit.Length;
                        return true;
                    }
                    return false;
                }
                // For token definitions, NameValue can appear only as nested collections or not at all; treat as fail.
                return false;

            case RegexValue rv:
            {
                var (pattern, options) = RegexTranslator.Translate(rv.Regex);
                if (!RegexCache.TryGetValue(pattern + "#" + (int)options, out var regex))
                {
                    regex = new Regex(@"\G(?:" + pattern + ")", options | RegexOptions.Compiled);
                    RegexCache[pattern + "#" + (int)options] = regex;
                }
                var m = regex.Match(text, pos);
                if (m.Success && m.Index == pos && m.Length > 0)
                {
                    length = m.Length;
                    return true;
                }
                return false;
            }

            case RangeValue rg:
            {
                if (!TryParseChar(rg.From, out var from) || !TryParseChar(rg.To, out var to))
                {
                    return false;
                }
                if (pos < text.Length)
                {
                    var ch = text[pos];
                    if (ch >= from && ch <= to)
                    {
                        length = 1;
                        return true;
                    }
                }
                return false;
            }

            case NamedValueList cv:
                foreach (var v in cv.Values)
                {
                    if (TryMatchValueText(v, text, pos, out var l) && l > 0)
                    {
                        length = l;
                        return true;
                    }
                }
                return false;

            default:
                return false;
        }
    }

    private void CollectLiterals(Expansion exp)
    {
        foreach (var expr in exp.Expressions)
        {
            var atom = GetAtom(expr);
            switch (atom)
            {
                case ValueAtom va when IsQuoted(va.Value is NameValue nv ? nv.Name : string.Empty):
                {
                    var name = ((NameValue)va.Value).Name;
                    _ = name; // silence analyzer
                    var lit = Unescape(Unquote(((NameValue)va.Value).Name));
                    if (!string.IsNullOrEmpty(lit)) _literals.Add(lit);
                    break;
                }
                case ParametersAtom pa:
                    foreach (var a in pa.Expansions) CollectLiterals(a.Expansion);
                    break;
                case ArrayAtom aa:
                    foreach (var a in aa.Expansions) CollectLiterals(a.Expansion);
                    break;
            }
        }
    }

    private static (int min, int? max) GetRepetition(Expression expr) =>
        expr switch
        {
            AtomOnlyExpression => (1, 1),
            AtomWithOperatorExpression aoe => aoe.Operator switch
            {
                "?" => (0, 1),
                "*" => (0, null),
                "+" => (1, null),
                _ => (1, 1)
            },
            TildeExpression te => (te.From, te.From),
            TildeRangeExpression tr => (tr.From, tr.To),
            _ => (1, 1)
        };

    private static Atom GetAtom(Expression expr) =>
        expr switch
        {
            AtomOnlyExpression a => a.Atom,
            AtomWithOperatorExpression a => a.Atom,
            TildeExpression t => t.Atom,
            TildeRangeExpression t => t.Atom,
            _ => throw new InvalidOperationException($"Unsupported expression type: {expr.GetType().Name}")
        };

    private static bool IsQuoted(string s) =>
        s.Length >= 2 && ((s[0] == '\"' && s[^1] == '\"') || (s[0] == '\'' && s[^1] == '\''));
    private static string Unquote(string s) => s.Substring(1, s.Length - 2);
    private static string Unescape(string s) =>
        s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\'", "'");

    private static bool TryParseChar(string raw, out char ch)
    {
        ch = '\0';
        if (IsQuoted(raw))
        {
            var s = Unescape(Unquote(raw));
            if (s.Length == 1) { ch = s[0]; return true; }
        }
        return false;
    }

    // ----------------------------
    //  Token-stream parser
    // ----------------------------

    private bool TryMatchRuleTokens(Rule rule, string input, List<TokenInstance> tokens, int ti, out ParseTreeNode? node, out int newTi)
    {
        CallStack.Push(rule.Name);
        var ok = TryMatchAlternationsTokens(rule.Name, rule.Expansions, input, tokens, ti, NodeKind.Rule, rule.Name, out node, out newTi);
        CallStack.Pop();
        return ok;
    }

    private bool TryMatchAlternationsTokens(string logicalName, Alias[] alternations, string input, List<TokenInstance> tokens, int ti, NodeKind kind, string? ruleName, out ParseTreeNode? node, out int newTi)
    {
        node = null;
        newTi = ti;

        foreach (var alias in alternations)
        {
            if (TryMatchExpansionTokens(alias.Expansion, input, tokens, ti, kind, out var child, out var ti2, ruleName, alias.Name))
            {
                var start = ti < tokens.Count ? tokens[ti].Start : (tokens.Count > 0 ? tokens[^1].Start + tokens[^1].Length : 0);
                var end = ti2 > ti ? tokens[ti2 - 1].Start + tokens[ti2 - 1].Length : start;
                node = new ParseTreeNode
                {
                    Name = string.IsNullOrEmpty(alias.Name) ? logicalName : alias.Name,
                    AliasName = alias.Name,
                    RuleName = ruleName,
                    Kind = string.IsNullOrEmpty(alias.Name) ? kind : NodeKind.Alias,
                    Start = start,
                    Length = Math.Max(0, end - start),
                    Text = input.Substring(start, Math.Max(0, end - start))
                };
                if (child != null) node.Children.Add(child);
                newTi = ti2;
                return true;
            }
        }

        RegisterExpected(ti < tokens.Count ? tokens[ti].Start : FarthestPos, logicalName);
        return false;
    }

    private bool TryMatchExpansionTokens(Expansion expansion, string input, List<TokenInstance> tokens, int ti, NodeKind kind, out ParseTreeNode? node, out int newTi, string? ruleName = null, string? aliasName = null)
    {
        node = null;
        newTi = ti;

        var children = new List<ParseTreeNode>();
        var i = ti;

        foreach (var expr in expansion.Expressions)
        {
            if (!TryMatchExpressionTokens(expr, input, tokens, i, out var exprNodes, out var i2))
            {
                FarthestPos = Math.Max(FarthestPos, i < tokens.Count ? tokens[i].Start : FarthestPos);
                return false;
            }
            if (exprNodes != null) children.AddRange(exprNodes);
            i = i2;
        }

        var start = ti < tokens.Count ? tokens[ti].Start : (tokens.Count > 0 ? tokens[^1].Start + tokens[^1].Length : 0);
        var end = i > ti ? tokens[i - 1].Start + tokens[i - 1].Length : start;

        node = new ParseTreeNode
        {
            Name = aliasName ?? ruleName ?? "(group)",
            Kind = aliasName != null ? NodeKind.Alias : (ruleName != null ? kind : NodeKind.Group),
            RuleName = ruleName,
            AliasName = aliasName,
            Start = start,
            Length = Math.Max(0, end - start),
            Text = input.Substring(start, Math.Max(0, end - start))
        };
        node.Children.AddRange(children);
        newTi = i;
        return true;
    }

    private bool TryMatchExpressionTokens(Expression expr, string input, List<TokenInstance> tokens, int ti, out List<ParseTreeNode>? nodes, out int newTi)
    {
        nodes = [];
        newTi = ti;

        var (min, max) = GetRepetition(expr);
        var atom = GetAtom(expr);

        var count = 0;
        while (!max.HasValue || count < max.Value)
        {
            if (!TryMatchAtomTokens(atom, input, tokens, newTi, out var node, out var ti2))
            {
                break;
            }
            if (node != null) nodes.Add(node);
            newTi = ti2;
            count++;
        }

        if (count < min)
        {
            RegisterExpected(ti < tokens.Count ? tokens[ti].Start : FarthestPos, DescribeAtom(atom));
            return false;
        }

        return true;
    }

    private string DescribeAtom(Atom atom) =>
        atom switch
        {
            ValueAtom va => DescribeValue(va.Value),
            ParametersAtom => "(group)",
            ArrayAtom => "(choice)",
            _ => "(unknown)"
        };

    private string DescribeValue(Value value) =>
        value switch
        {
            NameValue nv => nv.Name,
            RegexValue rv => rv.Regex,
            RangeValue rg => $"{rg.From}..{rg.To}",
            NamedValueList cv => cv.Name,
            _ => "(value)"
        };

    private bool TryMatchAtomTokens(Atom atom, string input, List<TokenInstance> tokens, int ti, out ParseTreeNode? node, out int newTi)
    {
        node = null;
        newTi = ti;

        switch (atom)
        {
            case ValueAtom va:
                return TryMatchValueTokens(va.Value, input, tokens, ti, out node, out newTi);

            case ParametersAtom pa:
                return TryMatchAlternationsTokens("(group)", pa.Expansions, input, tokens, ti, NodeKind.Group, null, out node, out newTi);

            case ArrayAtom aa:
                return TryMatchAlternationsTokens("(choice)", aa.Expansions, input, tokens, ti, NodeKind.Group, null, out node, out newTi);

            default:
                return false;
        }
    }

    private bool TryMatchValueTokens(Value value, string input, List<TokenInstance> tokens, int ti, out ParseTreeNode? node, out int newTi)
    {
        node = null;
        newTi = ti;

        switch (value)
        {
            case NameValue nv:
            {
                if (IsQuoted(nv.Name))
                {
                    var lit = Unescape(Unquote(nv.Name));
                    if (ti < tokens.Count && tokens[ti].IsLiteral && tokens[ti].Text == lit)
                    {
                        var t = tokens[ti];
                        node = new ParseTreeNode
                        {
                            Name = $"\"{lit}\"",
                            Kind = NodeKind.Literal,
                            Start = t.Start,
                            Length = t.Length,
                            Text = t.Text
                        };
                        newTi = ti + 1;
                        return true;
                    }
                    RegisterExpected(ti < tokens.Count ? tokens[ti].Start : FarthestPos, nv.Name);
                    return false;
                }

                // TOKEN?
                if (Tokens.ContainsKey(nv.Name))
                {
                    if (ti < tokens.Count && !tokens[ti].IsLiteral && tokens[ti].Name == nv.Name)
                    {
                        var t = tokens[ti];
                        node = new ParseTreeNode
                        {
                            Name = nv.Name,
                            TokenName = nv.Name,
                            Kind = NodeKind.Token,
                            Start = t.Start,
                            Length = t.Length,
                            Text = t.Text
                        };
                        newTi = ti + 1;
                        return true;
                    }
                    RegisterExpected(ti < tokens.Count ? tokens[ti].Start : FarthestPos, nv.Name);
                    return false;
                }

                // RULE?
                if (Rules.TryGetValue(nv.Name, out var rule))
                {
                    return TryMatchRuleTokens(rule, input, tokens, ti, out node, out newTi);
                }

                RegisterExpected(ti < tokens.Count ? tokens[ti].Start : FarthestPos, nv.Name);
                return false;
            }

            case RegexValue:
                // Regex terminals inside rule bodies are unsupported in LALR phase (they should be defined as TOKENs).
                return false;

            case RangeValue:
                // Ranges should be defined in TOKENs; treat as unsupported inside rule bodies in LALR phase.
                return false;

            case NamedValueList cv:
                foreach (var v in cv.Values)
                {
                    if (TryMatchValueTokens(v, input, tokens, ti, out node, out newTi))
                    {
                        return true;
                    }
                }
                return false;

            default:
                return false;
        }
    }
}

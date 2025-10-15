// TODO: Apologies, generated using an LLM, probably not the best approach.

using System.Text;
using System.Text.RegularExpressions;

namespace EtAlii.Ubigia.Lark;

internal sealed class EarleyParseAlgorithm : ParseAlgorithmBase, IParseAlgorithm
{
    private readonly Stack<Dictionary<string, Value>> _paramEnvStack = new();
    private Dictionary<string, Value>? CurrentEnv => _paramEnvStack.Count > 0 ? _paramEnvStack.Peek() : null;
    
    public EarleyParseAlgorithm(EbnfSyntax syntax, LarkParserConfiguration configuration)
        : base(syntax, configuration, "Earley")
    { 
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

        var success = TryMatchRule(startRule!, input, 0, out var node, out var pos);

        SkipIgnored(input, pos, out pos);

        var fullSuccess = success && pos == input.Length;

        var expected = ExpectedAtFarthest.ToArray();
        var diagnostics = MakeDiagnostics(input, AlgorithmName, FarthestPos, expected, CallStack.Reverse().ToArray());

        return new ParseResult
        {
            Success = fullSuccess,
            Input = input,
            Root = fullSuccess ? node : null,
            Errors = fullSuccess ? [] : [FormatErrorMessage(input, diagnostics)],
            Diagnostics = diagnostics
        };
    }
    

    private bool TryMatchRule(Rule rule, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        return TryMatchRule(rule, text, pos, null, out node, out newPos);
    }

    private bool TryMatchRule(Rule rule, string text, int pos, Dictionary<string, Value>? env, out ParseTreeNode? node, out int newPos)
    {
        if (env != null) _paramEnvStack.Push(env);
        CallStack.Push(rule.Name);
        var result = TryMatchAlternations(rule.Name, isTokenContext: false, rule.Expansions, text, pos, NodeKind.Rule, ruleName: rule.Name, out node, out newPos);
        CallStack.Pop();
        if (env != null) _paramEnvStack.Pop();
        return result;
    }

    private bool TryMatchToken(Token token, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        // Earley-style token choice: prefer the longest match (ties can be broken later by priority).
        node = null;
        newPos = pos;

        var bestLen = -1;
        ParseTreeNode? bestNode = null;
        var bestPos = pos;

        foreach (var a in token.Expansions)
        {
            if (TryMatchExpansion(a.Expansion, isTokenContext: true, text: text, startPos: pos, kind: NodeKind.Token, node: out var candidate, newPos: out var p2, ruleName: token.Name, aliasName: a.Name))
            {
                var len = p2 - pos;
                if (len > bestLen)
                {
                    bestLen = len;
                    bestNode = candidate;
                    bestPos = p2;
                }
            }
        }

        if (bestNode != null && bestLen > 0)
        {
            node = new ParseTreeNode
            {
                Name = token.Name,
                TokenName = token.Name,
                Kind = NodeKind.Token,
                Start = pos,
                Length = bestLen,
                Text = text.Substring(pos, bestLen)
            };
            node.Children.Add(bestNode);
            newPos = bestPos;
            return true;
        }

        RegisterExpected(pos, token.Name);
        return false;
    }

    private bool TryMatchAlternations(string logicalName, bool isTokenContext, Alias[] alternations, string text, int pos, NodeKind kind, string? ruleName, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        foreach (var alias in alternations)
        {
            if (TryMatchExpansion(alias.Expansion, isTokenContext, text, pos, kind, out var child, out var p2, ruleName, alias.Name))
            {
                var length = p2 - pos;
                node = new ParseTreeNode
                {
                    Name = string.IsNullOrEmpty(alias.Name) ? logicalName : alias.Name,
                    AliasName = alias.Name,
                    RuleName = ruleName,
                    Kind = string.IsNullOrEmpty(alias.Name) ? kind : NodeKind.Alias,
                    Start = pos,
                    Length = length,
                    Text = text.Substring(pos, length)
                };
                if (child != null) node.Children.Add(child);
                newPos = p2;
                return true;
            }
        }

        RegisterExpected(pos, logicalName);
        return false;
    }

    private bool TryMatchExpansion(Expansion expansion,
        bool isTokenContext,
        string text,
        int startPos,
        NodeKind kind,
        out ParseTreeNode? node,
        out int newPos,
        string? ruleName = null,
        string? aliasName = null)
    {
        node = null;
        newPos = startPos;

        if (!isTokenContext)
        {
            SkipIgnored(text, newPos, out newPos);
        }

        var children = new List<ParseTreeNode>();
        var p = newPos;

        foreach (var expr in expansion.Expressions)
        {
            if (!TryMatchExpression(expr, isTokenContext, text, p, out var exprNodes, out var p2))
            {
                UpdateFarthest(p);
                return false;
            }
            if (exprNodes != null) children.AddRange(exprNodes);
            p = p2;
            if (!isTokenContext) SkipIgnored(text, p, out p);
        }

        var len = p - newPos;
        node = new ParseTreeNode
        {
            Name = aliasName ?? ruleName ?? "(group)",
            Kind = aliasName != null ? NodeKind.Alias : (ruleName != null ? kind : NodeKind.Group),
            RuleName = ruleName,
            AliasName = aliasName,
            Start = newPos,
            Length = len,
            Text = text.Substring(newPos, len)
        };
        node.Children.AddRange(children);
        newPos = p;
        return true;
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

    private bool TryMatchExpression(Expression expr, bool isTokenContext, string text, int pos, out List<ParseTreeNode>? nodes, out int newPos)
    {
        nodes = [];
        newPos = pos;

        var (min, max) = GetRepetition(expr);
        var atom = GetAtom(expr);

        var count = 0;
        while (!max.HasValue || count < max.Value)
        {
            if (!TryMatchAtom(atom, isTokenContext, text, newPos, out var node, out var p2))
            {
                break;
            }
            if (node != null) nodes.Add(node);
            newPos = p2;
            count++;
            if (newPos >= text.Length) break;
            if (!isTokenContext) SkipIgnored(text, newPos, out newPos);
        }

        if (count < min)
        {
            RegisterExpected(pos, DescribeAtom(atom));
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

    private bool TryMatchAtom(Atom atom, bool isTokenContext, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        return atom switch
        {
            ValueAtom va => TryMatchValue(va.Value, text, pos, out node, out newPos),
            ParametersAtom pa => TryMatchAlternations("(group)", isTokenContext, pa.Expansions, text, pos, NodeKind.Group, ruleName: null, out node, out newPos),
            ArrayAtom aa => TryMatchAlternations("(choice)", isTokenContext, aa.Expansions, text, pos, NodeKind.Group, ruleName: null, out node, out newPos),
            _ => false
        };
    }

    private bool TryMatchValue(Value value, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        if (value is NameValue pnv && CurrentEnv != null && CurrentEnv.TryGetValue(pnv.Name, out var mapped))
        {
            return TryMatchValue(mapped, text, pos, out node, out newPos);
        }

        switch (value)
        {
            case NameValue nv:
                return TryMatchName(nv.Name, text, pos, out node, out newPos);

            case RegexValue rv:
                return TryMatchRegex(rv.Regex, text, pos, out node, out newPos);

            case RangeValue rg:
                return TryMatchRange(rg.From, rg.To, text, pos, out node, out newPos);

            case NamedValueList cv:
                foreach (var v in cv.Values)
                {
                    if (TryMatchValue(v, text, pos, out node, out newPos))
                    {
                        return true;
                    }
                }
                RegisterExpected(pos, cv.Name);
                return false;

            default:
                return false;
        }
    }

    private bool TryMatchName(string name, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        if (string.IsNullOrEmpty(name)) return false;

        if (CurrentEnv != null && CurrentEnv.TryGetValue(name, out var mapped))
        {
            return TryMatchValue(mapped, text, pos, out node, out newPos);
        }

        if (IsQuoted(name))
        {
            var lit = Unescape(Unquote(name));
            if (text.AsSpan(pos).StartsWith(lit.AsSpan(), StringComparison.Ordinal))
            {
                node = new ParseTreeNode
                {
                    Name = $"\"{lit}\"",
                    Kind = NodeKind.Literal,
                    Start = pos,
                    Length = lit.Length,
                    Text = text.Substring(pos, lit.Length)
                };
                newPos = pos + lit.Length;
                return true;
            }
            RegisterExpected(pos, name);
            return false;
        }

        if (Configuration.EnableParameterizedRules && TryParseParameterizedReference(name, out var baseName, out var args))
        {
            if (Rules.TryGetValue(baseName, out var prule))
            {
                var paramNames = prule.Parameters;
                if (paramNames.Length != args.Count)
                {
                    RegisterExpected(pos, $"{baseName}[{string.Join(",", paramNames)}]");
                    return false;
                }
                var env = new Dictionary<string, Value>(StringComparer.Ordinal);
                for (var i = 0; i < paramNames.Length; i++)
                {
                    env[paramNames[i]] = args[i];
                }
                return TryMatchRule(prule, text, pos, env, out node, out newPos);
            }
            RegisterExpected(pos, baseName);
            return false;
        }

        if (Tokens.TryGetValue(name, out var token))
        {
            return TryMatchToken(token, text, pos, out node, out newPos);
        }

        if (Rules.TryGetValue(name, out var rule))
        {
            return TryMatchRule(rule, text, pos, out node, out newPos);
        }

        RegisterExpected(pos, name);
        return false;
    }

    private bool TryMatchRegex(string rawPattern, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        var (pattern, options) = RegexTranslator.Translate(rawPattern);
        if (!RegexCache.TryGetValue(pattern + "#" + (int)options, out var regex))
        {
            regex = new Regex(@"\G(?:" + pattern + ")", options | RegexOptions.Compiled);
            RegexCache[pattern + "#" + (int)options] = regex;
        }

        var m = regex.Match(text, pos);
        if (m.Success && m.Index == pos && m.Length > 0)
        {
            node = new ParseTreeNode
            {
                Name = $"/{pattern}/",
                Kind = NodeKind.Regex,
                Start = pos,
                Length = m.Length,
                Text = m.Value
            };
            newPos = pos + m.Length;
            return true;
        }

        RegisterExpected(pos, rawPattern);
        return false;
    }

    private bool TryMatchRange(string fromRaw, string toRaw, string text, int pos, out ParseTreeNode? node, out int newPos)
    {
        node = null;
        newPos = pos;

        if (!TryParseChar(fromRaw, out var from) || !TryParseChar(toRaw, out var to))
        {
            RegisterExpected(pos, $"{fromRaw}..{toRaw}");
            return false;
        }

        if (pos < text.Length)
        {
            var ch = text[pos];
            if (ch >= from && ch <= to)
            {
                node = new ParseTreeNode
                {
                    Name = $"'{from}'..'{to}'",
                    Kind = NodeKind.Range,
                    Start = pos,
                    Length = 1,
                    Text = text.Substring(pos, 1)
                };
                newPos = pos + 1;
                return true;
            }
        }

        RegisterExpected(pos, $"{fromRaw}..{toRaw}");
        return false;
    }

    private static bool IsQuoted(string s) =>
        s.Length >= 2 && ((s[0] == '\"' && s[^1] == '\"') || (s[0] == '\'' && s[^1] == '\''));

    private static string Unquote(string s) => s.Substring(1, s.Length - 2);

    private static bool TryParseChar(string raw, out char ch)
    {
        ch = '\0';
        if (IsQuoted(raw))
        {
            var s = Unescape(Unquote(raw));
            if (s.Length == 1)
            {
                ch = s[0];
                return true;
            }
        }
        return false;
    }

    private static string Unescape(string s)
    {
        return s
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\\"", "\"")
            .Replace("\\'", "'");
    }

    private bool TryParseParameterizedReference(string name, out string baseName, out List<Value> args)
    {
        baseName = name;
        args = [];

        var allowBrackets = Configuration.ParamBrackets is ParameterBracketStyle.Brackets or ParameterBracketStyle.Both;
        var allowBraces = Configuration.ParamBrackets is ParameterBracketStyle.Braces or ParameterBracketStyle.Both;

        var idxBracket = allowBrackets ? name.IndexOf('[') : -1;
        var idxBrace = allowBraces ? name.IndexOf('{') : -1;

        int idx;
        char close;

        if (idxBracket >= 0 && (idxBrace < 0 || idxBracket < idxBrace))
        {
            idx = idxBracket; close = ']';
        }
        else if (idxBrace >= 0)
        {
            idx = idxBrace; close = '}';
        }
        else
        {
            return false;
        }

        baseName = name.Substring(0, idx);
        if (string.IsNullOrEmpty(baseName)) return false;

        if (name[^1] != close) return false;

        var inside = name.Substring(idx + 1, name.Length - idx - 2);
        args = ParseArgumentList(inside);
        return true;
    }

    private List<Value> ParseArgumentList(string s)
    {
        var result = new List<Value>();
        var current = new StringBuilder();
        bool inSingle = false, inDouble = false, inRegex = false;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (c == '\\' && i + 1 < s.Length)
            {
                current.Append(c);
                current.Append(s[++i]);
                continue;
            }
            if (!inRegex && !inSingle && c == '\"' && !inDouble) { inDouble = true; current.Append(c); continue; }
            if (!inRegex && inDouble && c == '\"') { inDouble = false; current.Append(c); continue; }
            if (!inRegex && !inDouble && c == '\'' && !inSingle) { inSingle = true; current.Append(c); continue; }
            if (!inRegex && inSingle && c == '\'') { inSingle = false; current.Append(c); continue; }
            if (!inSingle && !inDouble && c == '/' && !inRegex)
            {
                inRegex = true; current.Append(c); continue;
            }
            if (inRegex && c == '/')
            {
                current.Append(c);
                var j = i + 1;
                while (j < s.Length && char.IsLetter(s[j]))
                {
                    current.Append(s[j]);
                    j++;
                }
                i = j - 1;
                inRegex = false;
                continue;
            }

            if (!inSingle && !inDouble && !inRegex && c == ',')
            {
                var token = current.ToString().Trim();
                if (token.Length > 0) result.Add(ParseArgument(token));
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        var last = current.ToString().Trim();
        if (last.Length > 0) result.Add(ParseArgument(last));
        return result;
    }

    private Value ParseArgument(string token)
    {
        token = token.Trim();
        if (token.Length == 0) return new NameValue { Name = token };

        if (IsQuoted(token))
        {
            return new NameValue { Name = token };
        }

        if (token[0] == '/' && token.LastIndexOf('/') > 0)
        {
            return new RegexValue { Regex = token };
        }

        return new NameValue { Name = token };
    }

    private void SkipIgnored(string text, int pos, out int newPos)
    {
        var p = pos;
        var advanced = true;
        while (advanced)
        {
            advanced = false;
            foreach (var exp in IgnoreExpansions)
            {
                if (TryMatchExpansion(exp, isTokenContext: true, text: text, startPos: p, kind: NodeKind.Token, node: out _, newPos: out var p2, ruleName: null, aliasName: null))
                {
                    if (p2 > p)
                    {
                        p = p2;
                        advanced = true;
                        break;
                    }
                }
            }
        }
        newPos = p;
    }

    private void UpdateFarthest(int pos)
    {
        if (pos > FarthestPos) FarthestPos = pos;
    }
}

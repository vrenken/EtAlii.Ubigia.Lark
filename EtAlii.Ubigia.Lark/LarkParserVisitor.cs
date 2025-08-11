using System.Globalization;

namespace EtAlii.Ubigia.Lark;

public class LarkParserVisitor : LarkParserBaseVisitor<object>
{
    public override object VisitStart_(LarkParser.Start_Context context)
    {
        return context
            .item()
            .Select(i => (Item)base.VisitItem(i))
            .ToArray();
    }

    public override object VisitRule_(LarkParser.Rule_Context context)
    {
        var name = context.RULE().GetText();
        var parameters = (string[])VisitRule_params(context.rule_params());
        var priority = int.Parse(context.priority()?.NUMBER().GetText() ?? "0", CultureInfo.InvariantCulture);
        var expansions = (Alias[])VisitExpansions(context.expansions());
        return new Rule
        {
            Name = name,
            Parameters = parameters,
            Priority = priority,
            Expansions = expansions
        };
    }

    public override object VisitToken(LarkParser.TokenContext context)
    {
        var name = context.TOKEN().GetText();
        var parameters = (string[])VisitToken_params(context.token_params());
        var priority = int.Parse(context.priority()?.NUMBER().GetText() ?? "0", CultureInfo.InvariantCulture);
        var expansions = (Alias[])VisitExpansions(context.expansions());
        return new Token
        {
            Name = name,
            Parameters = parameters,
            Priority = priority,
            Expansions = expansions
        };
    }

    public override object VisitStatement(LarkParser.StatementContext context)
    {
        if (context.expansions() is { } expansions)
        {
            return new IgnoreStatement
            {
                Expansions = (Alias[])VisitExpansions(expansions)
            };
        }

        if (context.import_path() is { } importPath)
        {
            if (context.name_list() is { } nameList)
            {
                return new ImportStatement
                {
                    Path = importPath.GetText(),
                    Names = (string[])VisitName_list(nameList)
                };
            }

            return new ImportStatement
            {
                Path = importPath.GetText(),
                Names = context.name() is { Length: > 0 } name ? [ name[0].GetText() ] : []
            };
        }

        if (context.name() is { } names)
        {
            return new DeclareStatement
            {
                Names = names.Select(n => n.GetText()).ToArray()
            };
        }

        return new OverrideStatement
        {
            Rule = (Rule)VisitRule_(context.rule_())
        };
    }

    public override object VisitName_list(LarkParser.Name_listContext context)
    {
        return context
            .name()
            .Select(n => n.GetText())
            .ToArray();
    }

    public override object VisitExpansions(LarkParser.ExpansionsContext context)
    {
        return context.alias()
            .Select(a => (Alias)VisitAlias(a))
            .ToArray();
    }

    public override object VisitAlias(LarkParser.AliasContext context)
    {
        var expansion = (Expansion)VisitExpansion(context.expansion());
        var rule = context.RULE()?.GetText() ?? string.Empty;
        return new Alias
        {
            Expansion = expansion,
            Rule = rule,
        };
    }

    public override object VisitExpansion(LarkParser.ExpansionContext context)
    {
        var expressions = context
            .expr()
            .Select(e => (Expression)VisitExpr(e))
            .ToArray();
        return new Expansion
        {
            Expressions = expressions
        };
    }

    public override object VisitExpr(LarkParser.ExprContext context)
    {
        return new Expression
        {
            Atom = (Atom)VisitAtom(context.atom())
        };
    }

    public override object VisitRule_params(LarkParser.Rule_paramsContext context)
    {
        return context.RULE().Select(r => r.GetText()).ToArray();
    }

    public override object VisitName(LarkParser.NameContext context)
    {
        return context.GetText();
    }

    public override object VisitValue(LarkParser.ValueContext context)
    {
        var strings = context.STRING() ?? [];
        if (strings.Length == 2)
        {
            return new RangeValue
            {
                From = strings[0].GetText(),
                To = strings[1].GetText()
            };
        }

        if (context.REGEXP() is { } regexp)
        {
            return new RegexValue { Regex = regexp.GetText() };
        }

        var values = context.value();
        if (values.Length > 0)
        {
            var name = context.name().GetText();
            return new CollectionValue
            {
                Name = name,
                Values = values
                    .Select(v => (Value)VisitValue(v))
                    .ToArray()
            };
        }
        if (context.name() is { } n)
        {
            return new NameValue
            {
                Name = n.GetText()
            };
        }
        
        return new NameValue
        {
            Name = context.STRING(0).GetText()
        };
    }

    public override object VisitAtom(LarkParser.AtomContext context)
    {
        if (context.value() is { } value)
        {
            var v = (Value)VisitValue(value);
            return new ValueAtom { Value = v };
        }

        if (context.GetText().StartsWith("["))
        {
            return new ArrayAtom
            {
                Expansions = (Alias[])VisitExpansions(context.expansions())
            };
        }
        return new ParametersAtom
        {
            Expansions = (Alias[])VisitExpansions(context.expansions())
        };
    }
}
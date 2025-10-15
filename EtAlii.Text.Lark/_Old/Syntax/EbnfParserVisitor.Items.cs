using System.Globalization;

namespace EtAlii.Text.Lark._Old;

public partial class EbnfParserVisitor
{
    /// <inheritdoc />
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
    
    /// <inheritdoc />
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
    
    /// <inheritdoc />
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
            var fullPath = importPath.GetText();
            
            var importNames = context.name_list() is { } nameList
                ? (string[])VisitName_list(nameList)
                : context.name() is { Length: > 0 } name ? [name[0].GetText()] : [];
                
            var importStatement = new ImportStatement
            {
                Path = fullPath,
                Names = importNames
            };
        
            importStatement.Load(_importSource);
            return importStatement;

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
    
    /// <inheritdoc />
    public override object VisitName_list(LarkParser.Name_listContext context)
    {
        return context
            .name()
            .Select(n => n.GetText())
            .ToArray();
    }
    
    /// <inheritdoc />
    public override object VisitRule_params(LarkParser.Rule_paramsContext context)
    {
        return context.RULE().Select(r => r.GetText()).ToArray();
    }

    /// <inheritdoc />
    public override object VisitToken_params(LarkParser.Token_paramsContext context)
    {
        return context.TOKEN().Select(r => r.GetText()).ToArray();
    }
}
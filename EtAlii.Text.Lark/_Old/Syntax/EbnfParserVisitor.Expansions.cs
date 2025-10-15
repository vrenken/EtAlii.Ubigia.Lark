namespace EtAlii.Text.Lark._Old;

public partial class EbnfParserVisitor
{
    /// <inheritdoc />
    public override object VisitExpansions(LarkParser.ExpansionsContext context)
    {
        return context.alias()
            .Select(a => (Alias)VisitAlias(a))
            .ToArray();
    }

    /// <inheritdoc />
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
}
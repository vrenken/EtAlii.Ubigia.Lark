using System.Text;
using Antlr4.Runtime;

namespace EtAlii.Ubigia.Lark;

public partial class LarkParseSyntax
{
    /// Parses the contents of the provided stream and returns an array of parsed items.
    /// <param name="stream">
    /// The input stream containing the data to be parsed.
    /// </param>
    /// <returns>
    /// An array of parsed items derived from the input stream.
    /// </returns>
    public static LarkParseSyntax Parse(Stream stream)
    {
        var inputStream = new AntlrInputStream(stream);

        var lexer = new LarkLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new LarkParser(commonTokenStream)
        {
            BuildParseTree = true
        };
        var tree = parser.start_();
        //parser.CompileParseTreePattern()
        var visitor = new LarkParserVisitor();
        var items = (Item[])visitor.VisitStart_(tree);

        var errorListener = new LarkParserErrorListener();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener); // add ours
        
        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.AppendLine(item.ToString());
        }

        return new LarkParseSyntax
        {
            IsValid = errorListener.Errors.Count == 0,
            Errors = errorListener.Errors.ToArray(),
            Items = items,
            Text = sb.ToString().TrimEnd()
        };
    }
}
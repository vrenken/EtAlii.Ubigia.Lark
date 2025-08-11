using Antlr4.Runtime;

namespace EtAlii.Ubigia.Lark;

public partial class LarkParser
{
    /// Parses the contents of the provided stream and returns an array of parsed items.
    /// <param name="stream">
    /// The input stream containing the data to be parsed.
    /// </param>
    /// <returns>
    /// An array of parsed items derived from the input stream.
    /// </returns>
    public static Item[] Parse(Stream stream)
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
        return (Item[])visitor.VisitStart_(tree);
        //parser.RemoveErrorListeners();
        //parser.AddErrorListener(new LarkParserErrorListener()); // add ours
    }
}
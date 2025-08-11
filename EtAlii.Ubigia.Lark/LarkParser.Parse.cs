using Antlr4.Runtime;

namespace EtAlii.Ubigia.Lark;

public partial class LarkParser
{
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
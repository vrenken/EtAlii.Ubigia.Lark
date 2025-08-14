using System.Text;
using Antlr4.Runtime;

namespace EtAlii.Ubigia.Lark;

public partial class LarkParseSyntax
{
    /// <summary>
    /// Parses the content provided in the specified file and generates a <see cref="LarkParseSyntax"/> object.
    /// This method uses a default implementation of an import source for resolving references during parsing.
    /// </summary>
    /// <returns>A <see cref="LarkParseSyntax"/> object representing the parsed results, including items, errors, and the parsed text.</returns>
    public static LarkParseSyntax Parse(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName)!;
        var fileSystemImportSource = new FileSystemImportSource(directory);
        var cachingImportSource = new CachingImportSource(fileSystemImportSource);
        var stream = File.OpenRead(fileName);
        return Parse(stream, new CachingImportSource(cachingImportSource));
    }

    public static LarkParseSyntax Parse(TextReader reader, IImportSource importSource)
    {
        var inputStream = new AntlrInputStream(reader);
        return Parse(inputStream, importSource);
    }

    public static LarkParseSyntax Parse(Stream stream, IImportSource importSource)
    {
        var inputStream = new AntlrInputStream(stream);
        return Parse(inputStream, importSource);
    }

    /// <summary>
    /// Parses the content of a given stream into a <see cref="LarkParseSyntax"/> object.
    /// This method also allows specifying an additional import source for handling references.
    /// </summary>
    /// <param name="inputStream">The input stream containing the text to parse.</param>
    /// <param name="importSource">An implementation of the <see cref="IImportSource"/> interface used to resolve imports during parsing.</param>
    /// <returns>A <see cref="LarkParseSyntax"/> object containing the results of the parsing operation, including any errors and parsed items.</returns>
    public static LarkParseSyntax Parse(AntlrInputStream inputStream, IImportSource importSource)
    {
        var lexer = new LarkLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new LarkParser(commonTokenStream)
        {
            BuildParseTree = true
        };
        var tree = parser.start_();
        //parser.CompileParseTreePattern()
        var visitor = new LarkParserVisitor(importSource);
        var items = (Item[])visitor.VisitStart_(tree);

        var errorListener = new LarkParserErrorListener();
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener); // add ours
        
        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.AppendLine(item.ToString());
        }

        var importStatements = items
            .OfType<ImportStatement>()
            .ToArray();
        
        var allErrors = errorListener.Errors
            .Concat(importStatements.SelectMany(s => s.Syntax.Errors))
            .Distinct()
            .ToArray();
        
        return new LarkParseSyntax
        {
            HasStart = items.OfType<Rule>().Any(r => r.Name.EndsWith("start")),
            IsValid = allErrors.Length == 0,
            Errors = allErrors,
            Items = items,
            Text = sb.ToString().TrimEnd()
        };
    }
}
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
    public static LarkParseSyntax Parse(string fileName, string startRule = "start")
    {
        var directory = Path.GetDirectoryName(fileName)!;
        var fileSystemImportSource = new FileSystemImportSource(directory);
        var cachingImportSource = new CachingImportSource(fileSystemImportSource);
        var stream = File.OpenRead(fileName);
        return Parse(stream, new CachingImportSource(cachingImportSource), startRule);
    }

    public static LarkParseSyntax Parse(TextReader reader, IImportSource importSource, string startRule = "start")
    {
        var inputStream = new AntlrInputStream(reader);
        return Parse(inputStream, importSource, startRule);
    }

    public static LarkParseSyntax Parse(Stream stream, IImportSource importSource, string startRule = "start")
    {
        var inputStream = new AntlrInputStream(stream);
        return Parse(inputStream, importSource, startRule);
    }

    /// <summary>
    /// Parses the content from the specified input stream into a <see cref="LarkParseSyntax"/> object.
    /// The method enables providing a custom import source to resolve references during parsing and specifies a rule name to start parsing.
    /// </summary>
    /// <param name="inputStream">The input stream containing the content to be parsed.</param>
    /// <param name="importSource">An instance of <see cref="IImportSource"/> used to handle import references during parsing.</param>
    /// <param name="startRule">The name of the rule to use as the starting point for parsing. Defaults to "start".</param>
    /// <returns>A <see cref="LarkParseSyntax"/> object containing parsed content, rules, tokens, errors, and other detailed results of the operation.</returns>
    public static LarkParseSyntax Parse(
        AntlrInputStream inputStream, 
        IImportSource importSource,
        string startRule = "start")
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

        var imports = items
            .OfType<ImportStatement>()
            .ToArray();
        
        var allErrors = errorListener.Errors
            .Concat(imports.SelectMany(s => s.Syntax.Errors))
            .Distinct()
            .ToArray();
        
        var tokens = items.OfType<Token>().ToArray();
        var rules = items.OfType<Rule>().ToArray();
        var declares = items.OfType<DeclareStatement>().ToArray();
        var ignores = items.OfType<IgnoreStatement>().ToArray();
        var overrides = items.OfType<OverrideStatement>().ToArray();

        var start = rules.SingleOrDefault(r => r.Name.EndsWith(startRule));
        
        return new LarkParseSyntax
        {
            HasStart = start != null,
            Start = start,
            IsValid = allErrors.Length == 0,
            Errors = allErrors,
            Items = items,
            Rules = rules,
            Tokens = tokens,
            Declares = declares,
            Imports = imports,
            Ignores = ignores,
            Overrides = overrides,
            Text = sb.ToString().TrimEnd()
        };
    }
}
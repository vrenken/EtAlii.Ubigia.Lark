using Antlr4.Runtime;

namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Represents a custom error listener for the Lark parser.
/// It collects syntax errors encountered during parsing.
/// </summary>
public class LarkParserErrorListener : IAntlrErrorListener<IToken>
{
    public List<string> Errors { get; } = new();
    
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        Errors.Add($"{line}--{charPositionInLine}: {msg}");
    }
}
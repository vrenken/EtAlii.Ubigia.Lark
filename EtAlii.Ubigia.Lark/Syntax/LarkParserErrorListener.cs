using Antlr4.Runtime;

public class LarkParserErrorListener : IAntlrErrorListener<IToken>
{
    public List<string> Errors { get; } = new();
    
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
        string msg, RecognitionException e)
    {
        Errors.Add($"{line}--{charPositionInLine}: {msg}");
    }
}
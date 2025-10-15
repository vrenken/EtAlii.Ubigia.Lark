using System.Text;

// TODO: Apologies, generated using an LLM, probably not the best approach.

namespace EtAlii.Text.Lark._Old;

internal abstract partial class ParseAlgorithmBase
{
    protected ParseDiagnostics MakeDiagnostics(string input, string algorithmName, int pos, string[] expected, string[] callStack)
    {
        var (line, col, near) = GetLineColNear(input, pos);
        return new ParseDiagnostics
        {
            Algorithm = algorithmName,
            DynamicLexing = Configuration.Lexing == LexingMode.Dynamic,
            AmbiguityHandling = Configuration.Ambiguity,
            FarthestPosition = pos,
            Line = line,
            Column = col,
            NearText = near,
            Expected = expected,
            CallStack = callStack,
            Details = expected.Select(e => new DetailedError
            {
                Message = $"Expected {e}",
                Symbol = e,
                Position = pos,
                Line = line,
                Column = col
            }).ToArray()
        };
    }

    protected string FormatErrorMessage(string input, ParseDiagnostics d)
    {
        var sb = new StringBuilder();
        sb.Append($"Error at line {d.Line}, column {d.Column} (pos {d.FarthestPosition}) for input {input}");
        if (d.Expected.Length > 0)
        {
            sb.Append(": expected one of { ");
            sb.Append(string.Join(", ", d.Expected));
            sb.Append(" }");
        }
        sb.Append(".");
        return sb.ToString();
    }

    private (int line, int col, string near) GetLineColNear(string input, int pos)
    {
        pos = Math.Min(Math.Max(pos, 0), input.Length);
        var line = 1;
        var col = 1;
        var i = 0;
        while (i < pos)
        {
            if (input[i] == '\n') { line++; col = 1; }
            else { col++; }
            i++;
        }

        var previewStart = Math.Max(0, pos - 10);
        var previewEnd = Math.Min(input.Length, pos + 10);
        var near = input.Substring(previewStart, previewEnd - previewStart);
        return (line, col, near);
    }

}

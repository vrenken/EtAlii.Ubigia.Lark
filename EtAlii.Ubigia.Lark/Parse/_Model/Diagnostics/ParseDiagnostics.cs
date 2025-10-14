namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Detailed diagnostics for a parse attempt, similar to Lark.
/// </summary>
public class ParseDiagnostics
{
    public string Algorithm { get; init; } = "Unknown";
    public bool DynamicLexing { get; init; }
    public string AmbiguityHandling { get; init; } = "Default";

    public int FarthestPosition { get; init; } = 0;
    public int Line { get; init; } = 1;
    public int Column { get; init; } = 1;

    public string NearText { get; init; } = string.Empty;

    /// <summary>
    /// Expected terminals/non-terminals at the farthest error point.
    /// </summary>
    public string[] Expected { get; init; } = [];

    /// <summary>
    /// A light-weight call stack of active rules at the farthest error.
    /// </summary>
    public string[] CallStack { get; init; } = [];

    /// <summary>
    /// Additional technical details per error.
    /// </summary>
    public DetailedError[] Details { get; init; } = [];
}
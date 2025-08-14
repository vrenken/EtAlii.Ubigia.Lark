namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Represents an import statement in the Lark syntax.
/// </summary>
/// <remarks>
/// An import statement specifies modules or symbols to be imported from other Lark files.
/// </remarks>
public record ImportStatement : Statement
{
    /// <summary>
    /// Gets the syntax representation associated with the import statement.
    /// </summary>
    /// <remarks>
    /// This property holds the parsed syntax tree for an import statement in the Lark syntax.
    /// It is loaded by parsing the content of the imported file and interpreting its structure.
    /// </remarks>
    public LarkParseSyntax Syntax { get; private set; } = null!;

    /// <summary>
    /// Gets the path associated with the import statement.
    /// </summary>
    /// <remarks>
    /// This property specifies the module or file path to be imported in the Lark syntax. It is used to locate the
    /// resource referenced within the import statement and plays a critical role in resolving dependencies.
    /// </remarks>
    public required string Path { get; init; }
    public required string?[] Names { get; init; }
    
    /// <inheritdoc />
    public override string ToString()
    {
        return Names.Length switch
        {
            0 => $"%import {Path}",
            1 => $"%import {Path} -> {Names[0]}",
            _ => $"%import {Path} ({string.Join(',', Names)})"
        };
    }

    internal void Load(IImportSource importSource)
    {
        var filename = Path.Split('.')[0];
        filename = $"{filename}.lark";
        var content = importSource.Import(filename);
        using var s = new StringReader(content);
        Syntax = LarkParseSyntax.Parse(s, importSource);
    }
}
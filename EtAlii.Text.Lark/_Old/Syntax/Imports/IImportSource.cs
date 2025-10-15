namespace EtAlii.Text.Lark._Old;

/// <summary>
/// Represents a source from which other syntax definitions can be imported.
/// </summary>
/// <remarks>
/// Implementations of this interface provide specific import mechanisms,
/// such as reading from a file system or utilizing cached data.
/// </remarks>
public interface IImportSource
{
    /// <summary>
    /// Imports the content from a specified source file.
    /// </summary>
    /// <param name="fileName">The name of the file to be imported.</param>
    /// <returns>The content of the file as a string.</returns>
    public EbnfSyntax Import(string fileName);
}
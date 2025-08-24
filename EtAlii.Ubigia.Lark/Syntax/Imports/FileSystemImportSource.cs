namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Represents an import source for fetching and parsing EBNF syntax files from the file system.
/// </summary>
public class FileSystemImportSource : IImportSource
{
    private readonly string _currentDirectory;
    private readonly string _commonDirectory;

    /// <summary>
    /// Provides functionality to load and process EBNF syntax definitions from the file system.
    /// This class allows specifying directories for the current and shared resources.
    /// </summary>
    /// <remarks>
    /// Primarily acts as an implementation of the <see cref="IImportSource"/> interface,
    /// responsible for locating and retrieving files required during the parsing process.
    /// </remarks>
    public FileSystemImportSource(string currentDirectory, string commonDirectory)
    {
        _currentDirectory = currentDirectory;
        _commonDirectory = commonDirectory;
    }

    /// <inheritdoc />
    public EbnfSyntax Import(string fileName)
    {
        string fullPath;
        if (fileName.StartsWith('.'))
        {
            fileName = fileName.TrimStart('.');
            fileName = fileName.Split('.')[0];
            fileName = $"{fileName}.lark";
            fullPath = Path.Combine(_currentDirectory, fileName);
        }
        else
        {
            fileName = fileName.Split('.')[0];
            fileName = $"{fileName}.lark";
            fullPath = Path.Combine(_commonDirectory, fileName);
        }
        
        var content = File.ReadAllText(fullPath);
        using var s = new StringReader(content);
        return EbnfSyntax.Parse(s);
    }
}
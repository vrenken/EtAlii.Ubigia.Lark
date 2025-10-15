namespace EtAlii.Text.Lark._Old;

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
            var pieces = fileName.Split('.');
            var _ = pieces.Length > 1 ? pieces[^1] : null;
            pieces = pieces.Take(pieces.Length > 1 ? pieces.Length - 1 : 1).ToArray();
            fileName = string.Join(Path.DirectorySeparatorChar, pieces);
            fileName = $"{fileName}.lark";
            fullPath = Path.Combine(_currentDirectory, fileName);

            // if (!File.Exists(fullPath))
            // {
            //     fullPath = Path.Combine(_commonDirectory, fileName); 
            // }
        }
        else
        {
            var pieces = fileName.Split('.');
            var _ = pieces.Length > 1 ? pieces[^1] : null;
            pieces = pieces.Take(pieces.Length > 1 ? pieces.Length - 1 : 1).ToArray();
            fileName = string.Join(Path.DirectorySeparatorChar, pieces);
            //fileName = fileName.Split('.')[0];
            //fileName = fileName.Replace('.', Path.PathSeparator);
            fileName = $"{fileName}.lark";
            fullPath = Path.Combine(_commonDirectory, fileName);
            if (!File.Exists(fullPath))
            {
                // TODO: This sounds false but is needed for some of our test files.
                fullPath = Path.Combine(_currentDirectory, fileName); 
            }
        }
        
        var content = File.ReadAllText(fullPath);
        using var s = new StringReader(content);
        return EbnfSyntax.Parse(s, this);
    }
}
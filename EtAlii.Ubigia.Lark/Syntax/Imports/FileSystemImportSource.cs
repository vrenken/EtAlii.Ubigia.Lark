namespace EtAlii.Ubigia.Lark;

public class FileSystemImportSource : IImportSource
{
    private readonly string _directory;

    public FileSystemImportSource(string directory)
    {
        _directory = directory;
    }

    public string Import(string fileName)
    {
        var fullPath = Path.Combine(_directory, fileName);
        return File.ReadAllText(fullPath);
    }
}
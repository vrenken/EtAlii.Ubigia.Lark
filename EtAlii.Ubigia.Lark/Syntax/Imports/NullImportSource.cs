namespace EtAlii.Ubigia.Lark;

public class NullImportSource : IImportSource
{
    public string Import(string fileName)
    {
        throw new InvalidOperationException("Importing files is not supported.");
    }
}
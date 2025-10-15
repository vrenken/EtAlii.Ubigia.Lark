namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Represents a null import source implementation for cases where no importing is required or allowed.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IImportSource"/> interface and provides
/// a mechanism to explicitly disallow importing external syntax by throwing
/// an exception whenever an import operation is attempted.
/// </remarks>
public class NullImportSource : IImportSource
{
    /// <inheritdoc />
    public EbnfSyntax Import(string fileName)
    {
        throw new InvalidOperationException("Importing files is not supported.");
    }
}
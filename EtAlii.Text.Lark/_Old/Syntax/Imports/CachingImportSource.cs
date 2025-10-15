namespace EtAlii.Text.Lark._Old;

/// <summary>
/// Provides caching capabilities to an underlying import source implementation, reducing redundant fetch operations.
/// </summary>
/// <remarks>
/// This class decorates an existing <see cref="IImportSource"/> by introducing a cache layer.
/// When an import operation is requested, the content is first retrieved from the cache if available. If not,
/// the decorated import source fetches the content, which is then stored in the cache for subsequent requests.
/// </remarks>
public class CachingImportSource : IImportSource
{
    private readonly Dictionary<string, EbnfSyntax> _cache = new();
    private readonly IImportSource _inner
        ;

    /// <summary>
    /// Provides caching functionality to optimize the import process by reducing redundant fetch operations.
    /// </summary>
    /// <remarks>
    /// This class acts as a decorator for the <see cref="IImportSource"/> interface, introducing a caching mechanism to
    /// store previously fetched syntax definitions. On an import request, the cache is checked for existing data before
    /// delegating to the underlying import source.
    /// </remarks>
    public CachingImportSource(IImportSource inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public EbnfSyntax Import(string fileName)
    {
        if (!_cache.TryGetValue(fileName, out var syntax))
        {
            syntax = _cache[fileName] = _inner.Import(fileName);
        }
        return syntax;
    }
}
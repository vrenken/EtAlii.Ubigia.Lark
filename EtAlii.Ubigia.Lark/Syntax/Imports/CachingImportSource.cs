namespace EtAlii.Ubigia.Lark;

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
    private readonly Dictionary<string, string> _cache = new();
    private readonly IImportSource _inner
        ;

    public CachingImportSource(IImportSource inner)
    {
        _inner = inner;
    }

    public string Import(string fileName)
    {
        if (!_cache.TryGetValue(fileName, out var content))
        {
            content = _cache[fileName] = _inner.Import(fileName);
        }
        return content;
    }
}
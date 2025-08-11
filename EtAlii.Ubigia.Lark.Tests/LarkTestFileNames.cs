using System.Collections;

namespace EtAlii.Ubigia.Lark.Tests;

public class LarkTestFileNames : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        return Directory
            .GetFiles("_Examples", "*.lark", SearchOption.AllDirectories)
            .Select(fn => new object[] { fn })
            .ToList()
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
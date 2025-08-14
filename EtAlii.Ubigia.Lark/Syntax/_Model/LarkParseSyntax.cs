using System.Text;

namespace EtAlii.Ubigia.Lark;

public partial class LarkParseSyntax
{
    public required bool IsValid { get; init; }

    public required Item[] Items { get; init; }

    public required string[] Errors { get; init; }

    public required string Text { get; init; }
}
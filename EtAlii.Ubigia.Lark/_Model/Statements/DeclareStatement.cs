namespace EtAlii.Ubigia.Lark;

public record DeclareStatement : Statement
{
    public required string[] Names { get; init; }
}
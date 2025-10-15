namespace EtAlii.OpenAI.Tests;

public record SidcRefinementResult
{
    public required SidcRefinementOption[] Options { get; set; }
    public required bool KeepRefining { get; set; }
}
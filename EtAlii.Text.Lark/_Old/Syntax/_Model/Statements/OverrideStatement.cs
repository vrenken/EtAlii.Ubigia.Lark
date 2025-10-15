namespace EtAlii.Text.Lark._Old;

public record OverrideStatement : Statement
{
    public required Rule Rule { get; init; }

    public override string ToString()
    {
        return $"%override {Rule}";
    }
}
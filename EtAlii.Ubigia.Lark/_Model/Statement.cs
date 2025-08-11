namespace EtAlii.Ubigia.Lark;

public abstract record Statement : Item
{
    
}

public record OverrideStatement : Statement
{
    public required Rule Rule { get; init; }

    public override string ToString()
    {
        return $"%override {Rule}";
    }
}
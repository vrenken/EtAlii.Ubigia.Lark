namespace EtAlii.Text.Lark._Old;

public record Rule : Item
{
    public required string Name { get; init; }
    public required string[] Parameters { get; init; }
    
    public required int Priority { get; init; }
    public required Alias[] Expansions { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        var priority = Priority == 0 ? "" : $".{Priority}";
        return Parameters.Any()
            ? $"{Name}{{{string.Join(", ", Parameters)}}}{priority}: {string.Join($"{Environment.NewLine}\t| ", Expansions.Select(e => e.ToString()))}"
            : $"{Name}{priority}: {string.Join($"{Environment.NewLine}\t| ", Expansions.Select(e => e.ToString()))}";
    }
}
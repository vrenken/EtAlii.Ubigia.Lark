namespace EtAlii.OpenAI.Tests;

public record Organisation : Entity
{
    public required string EchelonLevel { get; set; }
    public List<Entity> Children { get; init; } = new();
    
    public static Entity[] Flatten(IEnumerable<Entity> organisations)
    {
        var result = new List<Entity>();
        
        foreach (var child in organisations)
        {
            result.Add(child);

            if (child is not Organisation organisation) continue;
            var children = Flatten(organisation.Children);
            result.AddRange(children);
        }
        return result.ToArray();
    }
}
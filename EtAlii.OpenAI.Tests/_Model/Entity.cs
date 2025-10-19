namespace EtAlii.OpenAI.Tests;

public abstract record Entity
{
    public required int Id { get; init; }
    public required string Name { get; set; }
    public required string Sidc { get; set; }
    public required string Description { get; set; }
}
namespace EtAlii.Ubigia.Lark;

public record ImportStatement : Statement
{
    public required string Path { get; init; }
    public required string?[] Names { get; init; }
    
    public override string ToString()
    {
        return Names.Length switch
        {
            0 => $"%import {Path}",
            1 => $"%import {Path} => {Names[0]}",
            _ => $"%import {Path} ({string.Join(',', Names)})"
        };
    }
}
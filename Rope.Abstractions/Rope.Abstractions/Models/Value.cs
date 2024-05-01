namespace Rope.Abstractions.Models;
public record Value
{
    public required string Type { get; set; }
    public required string[] Params { get; set; }
}

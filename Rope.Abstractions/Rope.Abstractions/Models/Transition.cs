namespace Rope.Abstractions.Models;
public class Transition
{
    public required string Condition { get; set; }
    public required string[] Branches { get; set; }
}

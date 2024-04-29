namespace RopeCSharp.Models;
internal class Transition
{
    public required string Condition { get; set; }
    public required string[] Branches { get; set; }
}

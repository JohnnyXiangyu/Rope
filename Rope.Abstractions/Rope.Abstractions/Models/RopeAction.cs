namespace Rope.Abstractions.Models;
public class RopeAction
{
    public required string Action { get; set; }
    public required List<RopeValue> Values { get; set; }
}

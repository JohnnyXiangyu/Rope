
namespace Rope.Abstractions.Models;
public class RopeScript
{
    public required string Context { get; set; }
    public required string Namespace { get; set; }
    public required List<RopeNode> Nodes { get; set; }
    public required string Name { get; set; }
}

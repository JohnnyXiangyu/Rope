
namespace Rope.Abstractions.Models;
public class Script
{
    public required string Context { get; set; }
    public required string Namespace { get; set; }
    public required List<ScriptNode> Nodes { get; set; }
    public required string Name { get; set; }
}

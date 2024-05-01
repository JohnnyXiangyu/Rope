
namespace Rope.Abstractions.Models;
public class Script
{
    public required string Context { get; set; }
    public required string Namespace { get; set; }
    public required List<Node> Nodes { get; set; }
    public required string Name { get; set; }

    public static async Task<Script> DeserializeAsync(string script)
    {
        throw new NotImplementedException();
    }
}

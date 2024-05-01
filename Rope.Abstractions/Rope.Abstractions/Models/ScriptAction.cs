namespace Rope.Abstractions.Models;
public class ScriptAction
{
    public required string Action { get; set; }
    public required List<Value> Values { get; set; }
}

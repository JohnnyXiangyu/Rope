namespace Rope.Abstractions.Models;
public record Node
{
    public required string Name { get; set; }
    public required List<ScriptAction> Actions { get; set; }
    public Transition? Transition { get; set; } = null;
}

namespace Rope.Abstractions.Models;
public class ScriptNode
{
    public required string Name { get; set; }
    public required float PosX { get; set; }
    public required float PosY { get; set; }
    public required ScriptAction[] Actions { get; set; }
    public Transition? Transition { get; set; } = null;
}

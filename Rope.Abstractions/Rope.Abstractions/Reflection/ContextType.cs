namespace Rope.Abstractions.Reflection;
public class ContextType
{
    public required string Name { get; set; }
    public required string? ModuleReq { get; set; }
    public required ContextAction[] Actions { get; set; }
}

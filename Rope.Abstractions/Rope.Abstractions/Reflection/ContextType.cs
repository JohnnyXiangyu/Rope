namespace Rope.Abstractions.Reflection;
public class ContextType
{
    public required string Name { get; set; }
    public required string? ModuleReq { get; set; }
    public required Dictionary<string, ContextAction> Actions { get; set; }
}

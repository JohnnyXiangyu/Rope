namespace Rope.Abstractions.Reflection;
public record ContextType
{
    public required string Name { get; set; }
    public required string? ModuleReq { get; set; }
    public required IEnumerable<ContextAction> Actions { get; set; }
}

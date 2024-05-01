namespace Rope.Abstractions.Reflection;

public record ContextAction
{
    public required string Name { get; set; }
    public required IEnumerable<DataConstructor> Params { get; set; }
}

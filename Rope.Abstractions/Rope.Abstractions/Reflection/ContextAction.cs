namespace Rope.Abstractions.Reflection;

public class ContextAction
{
    public required string Name { get; set; }
    public required Parameter[] Params { get; set; }
}

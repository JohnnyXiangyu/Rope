namespace Rope.Abstractions.Reflection;

public enum LiteralType
{
    String,
    Int32,
    Int64,
    Float,
    Double,
    Boolean
}

public class DataConstructor
{
    public required string Name { get; set; }
    public required bool IsArray { get; set; }
    public required LiteralType[] Params { get; set; }
}

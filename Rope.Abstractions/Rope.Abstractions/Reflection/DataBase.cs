namespace Rope.Abstractions.Reflection;
public record DataBase
{
    public required Dictionary<string, ContextType> ContextTypes { get; set; }
    public required Dictionary<string, DataConstructor> Constructors { get; set; }
}

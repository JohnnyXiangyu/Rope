using RopeCSharp.Serialization;

namespace RopeCSharp.Models;
internal record Node
{
    public required string Name { get; set; }
    public required List<ScriptAction> Actions { get; set; }

    public void Serialize(SerializationContext context)
    {
        // process the initial lines of a file
        using IDisposable methodScope = context.StartScope($"public void {Name}()");

        // process each action
        foreach (ScriptAction action in Actions)
        {
            action.Serialize(context);
        }
    }
}

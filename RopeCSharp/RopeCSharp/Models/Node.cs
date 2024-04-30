using RopeCSharp.Serialization;

namespace RopeCSharp.Models;
internal record Node
{
    public required string Name { get; set; }
    public required List<ScriptAction> Actions { get; set; }
    public Transition? Transition { get; set; } = null;

    public void Serialize(SerializationContext context)
    {
        // process the initial lines of a file
        using Scope methodScope = context.StartScope($"public IEnumerable {Name}()");

        // process each action
        foreach (ScriptAction action in Actions)
        {
            action.Serialize(context);
            context.AppendLine("yield return null;");
        }

        // at the end of a node is its transition
        EncodeTransition(context);
    }

    private void EncodeTransition(SerializationContext context)
    {
        // missing transition means terminate
        if (Transition == null)
        {
            context.AppendLine("NextState = States.Terminate;");
            return;
        }

        using Scope switchScope = context.StartScope($"switch (context.{Transition.Condition})");
        for (int i = 0; i < Transition.Branches.Length; i++)
        {
            using Scope caseScope = context.StartScope($"case {i}:");
            context.AppendLine($"NextState = States.{Transition.Branches[i]};");
            context.AppendLine("break;");
        }

        // fall through
        using Scope terminateScope = context.StartScope($"default:");
        context.AppendLine("NextState = States.Terminate;");
        context.AppendLine("break;");
    }
}

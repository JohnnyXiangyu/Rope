using Rope.Abstractions.Models;
using RopeCSharp.Serialization;

namespace RopeCSharp.Extensions;
internal static class NodeExtensions
{
    public static void Serialize(this ScriptNode self, SerializationContext context)
    {
        // process the initial lines of a file
        using Scope methodScope = context.StartScope($"public IEnumerable {self.Name}()");

        // process each action
        foreach (ScriptAction action in self.Actions)
        {
            action.Serialize(context);
            context.AppendLine("yield return null;");
        }

        // at the end of a node is its transition
        self.EncodeTransition(context);
    }

    private static void EncodeTransition(this ScriptNode self, SerializationContext context)
    {
        // missing transition means terminate
        if (self.Transition == null)
        {
            context.AppendLine("NextState = States.Terminate;");
            return;
        }

        using Scope switchScope = context.StartScope($"switch (context.{self.Transition.Condition})");
        for (int i = 0; i < self.Transition.Branches.Count; i++)
        {
            using Scope caseScope = context.StartScope($"case {i}:");
            context.AppendLine($"NextState = States.{self.Transition.Branches[i]};");
            context.AppendLine("break;");
        }

        // fall through
        using Scope terminateScope = context.StartScope($"default:");
        context.AppendLine("NextState = States.Terminate;");
        context.AppendLine("break;");
    }
}

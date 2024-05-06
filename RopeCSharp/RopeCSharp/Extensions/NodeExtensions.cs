using Rope.Abstractions.Models;
using RopeCSharp.Serialization;

namespace RopeCSharp.Extensions;
internal static class NodeExtensions
{
    public static void Serialize(this RopeNode self, SerializationContext context)
    {
        // process the initial lines of a file
        using Scope methodScope = context.StartScope($"public IEnumerable {self.Name}()");

        // process each action
        foreach (RopeAction action in self.Actions)
        {
            action.Serialize(context);
            context.AppendLine("yield return null;");
        }

        // at the end of a node is its transition
        self.EncodeTransition(context);
    }

    private static void EncodeTransition(this RopeNode self, SerializationContext context)
    {
        // missing transition means terminate
        if (self.Condition == string.Empty || self.Branches.Count <= 0)
        {
            context.AppendLine("NextState = States.Terminate;");
            return;
        }

        using Scope switchScope = context.StartScope($"switch (context.{self.Condition})");
        for (int i = 0; i < self.Branches.Count; i++)
        {
            using Scope caseScope = context.StartScope($"case {i}:");
            context.AppendLine($"NextState = States.{self.Branches[i]};");
            context.AppendLine("break;");
        }

        // fall through
        using Scope terminateScope = context.StartScope($"default:");
        context.AppendLine("NextState = States.Terminate;");
        context.AppendLine("break;");
    }
}

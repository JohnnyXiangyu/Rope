using RopeCSharp.Serialization;
using System.Text;
using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;

namespace RopeCSharp.Extensions;
internal static class ScriptActionExtensions
{
    public static void Serialize(this RopeAction self, SerializationContext context)
    {
        StringBuilder builder = new();
        ContextAction archeType = context.SelectedContextType!.Actions[self.Action];

        if (self.Values.Count != archeType.Params.Length)
            throw new Exception($"unexpected number of params encountered for action {self.Action}");

        builder.Append($"context.{self.Action}(");
        bool first = true;

        for (int i = 0; i < archeType.Params.Length; i ++)
        {
            if (!first)
            {
                builder.Append(", ");
            }
            first = false;

            if (archeType.Params[i].Type == LiteralType.String)
            {
                builder.Append($"\"{self.Values[i]}\"");
            }
            else
            {
                builder.Append(self.Values[i]);
            }
        }
        builder.Append(");");
        context.AppendLine(builder.ToString());
    }
}

using RopeCSharp.Serialization;
using System.Text;

namespace RopeCSharp.Models;
internal class ScriptAction
{
    public required string Action {  get; set; }
    public required List<string> Values { get; set; }

    public void Serialize(SerializationContext context)
    {
        StringBuilder builder = new();
        builder.Append($"context.{Action}(");
        bool first = true;
        foreach (string value in Values)
        {
            if (!first)
            {
                builder.Append(", ");
            }
            first = false;
            builder.Append($"{value}");
        }
        builder.Append(");");
        context.AppendLine(builder.ToString());
    }
}

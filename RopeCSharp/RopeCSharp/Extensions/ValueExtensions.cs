using Rope.Abstractions.Models;
using System.Text;

namespace RopeCSharp.Extensions;
internal static class ValueExtensions
{
    public readonly static HashSet<string> RawPrimTypes = [typeof(int).Name, typeof(long).Name, typeof(bool).Name, typeof(float).Name, typeof(double).Name];

    public static string SerializeAsLiteral(this Value self)
    {
        if (self.Type == typeof(string).Name)
        {
            return $"\"{self.Params[0]}\"";
        }
        else if (RawPrimTypes.Contains(self.Type))
        {
            return self.Params[0];
        }
        else
        {
            // for standard constructor types
            StringBuilder builder = new();
            bool first = false;
            foreach (string param in self.Params)
            {
                if (!first)
                {
                    builder.Append(", ");
                }
                first = false;
                builder.Append($"{param}");
            }
            return $"new {self.Type}({builder})";
        }
    }
}

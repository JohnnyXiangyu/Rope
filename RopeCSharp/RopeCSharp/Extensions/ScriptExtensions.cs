using RopeCSharp.Serialization;
using System.Text.Json;
using Rope.Abstractions.Models;

namespace RopeCSharp.Extensions;
internal static class ScriptExtensions
{
    public static async Task<RopeScript> DeserializeAsync(string path)
    {
        using FileStream scriptFile = File.OpenRead(path);
        RopeScript script = (await JsonSerializer.DeserializeAsync<RopeScript>(scriptFile).ConfigureAwait(false)) ?? throw new ArgumentException("input path doesn't contain valid script");
        return script;
    }

    public static void Serialize(this RopeScript self, SerializationContext serializationContext)
    {
        // using
        if (!serializationContext.ContextTypes.TryGetValue(self.Context, out Type? contextType))
        {
            throw new Exception($"invalid context type {self.Context}");
        }
        serializationContext.AppendLine($"using {contextType.Namespace};");
        serializationContext.AppendLine("using System.Collections;");

        // namespace
        serializationContext.AppendLine($"namespace {self.Namespace};");

        // script as class
        using Scope classScope = serializationContext.StartScope($"public class {self.Name}({self.Context} context)");

        // state management
        using (Scope enumScope = serializationContext.StartScope("public enum States"))
        {
            foreach (RopeNode node in self.Nodes)
            {
                serializationContext.AppendLine($"{node.Name},");
            }
            serializationContext.AppendLine("Terminate");
        }

        serializationContext.AppendLine($"public States NextState {{ get; private set; }} = States.{self.Nodes[0].Name};");

        // nodes as methods
        foreach (RopeNode node in self.Nodes)
        {
            node.Serialize(serializationContext);
        }

        // run method
        using Scope runMethodScope = serializationContext.StartScope("public IEnumerable Run()");
        using Scope whileLoopScope = serializationContext.StartScope("while (true)");
        using (Scope ifScope = serializationContext.StartScope("if (NextState == States.Terminate)"))
        {
            serializationContext.AppendLine("break;");
        }
        using Scope switchStateScope = serializationContext.StartScope("switch (NextState)");
        foreach (RopeNode node in self.Nodes)
        {
            using Scope caseScope = serializationContext.StartScope($"case States.{node.Name}:");
            using (Scope foreachScope = serializationContext.StartScope($"foreach (object? _ in {node.Name}())"))
            {
                serializationContext.AppendLine("yield return null;");
            }
            serializationContext.AppendLine("break;");
        }
    }
}

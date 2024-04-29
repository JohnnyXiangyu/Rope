using RopeCSharp.Serialization;
using System.Text.Json;

namespace RopeCSharp.Models;
internal class Script
{
    public required string Context { get; set; }
    public required string Namespace { get; set; }
    public required List<Node> Nodes { get; set; }
    public required string Name { get; set; }

    public static async Task<Script> DeserializeAsync(string path)
    {
        using FileStream scriptFile = File.OpenRead(path);
        Script script = (await JsonSerializer.DeserializeAsync<Script>(scriptFile).ConfigureAwait(false)) ?? throw new ArgumentException("input path doesn't contain valid script");
        return script;
    }

    public void Serialize(SerializationContext serializationContext)
    {
        // using
        if (!serializationContext.ContextTypes.TryGetValue(Context, out Type? contextType))
        {
            throw new Exception($"invalid context type {Context}");
        }
        serializationContext.AppendLine($"using {contextType.Namespace};");

        // namespace
        serializationContext.AppendLine($"namespace {Namespace};");

        // class
        using IDisposable classScope = serializationContext.StartScope($"public class {Name}({Context} context)");
        foreach (Node node in Nodes)
        {
            node.Serialize(serializationContext);
        }

        // entry method
        using IDisposable runMethodScope = serializationContext.StartScope("public void Run()");
        serializationContext.AppendLine($"{Nodes.First().Name}();");
    }
}

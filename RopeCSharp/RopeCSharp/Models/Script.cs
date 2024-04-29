﻿using RopeCSharp.Serialization;
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

        // script as class
        using Scope classScope = serializationContext.StartScope($"public class {Name}({Context} context)");

        // state management
        using (Scope enumScope = serializationContext.StartScope("public enum States"))
        {
            foreach (Node node in Nodes)
            {
                serializationContext.AppendLine($"{node.Name},");
            }
            serializationContext.AppendLine("Terminate");
        }

        serializationContext.AppendLine($"public States NextState {{ get; private set; }} = States.{Nodes[0].Name};");

        // nodes as methods
        foreach (Node node in Nodes)
        {
            node.Serialize(serializationContext);
        }

        // entry method
        using Scope runMethodScope = serializationContext.StartScope("public void Run()");
        using Scope whileLoopScope = serializationContext.StartScope("while (true)");
        using Scope switchStateScope = serializationContext.StartScope("switch (NextState)");
        foreach (Node node in Nodes)
        {
            using Scope caseScope = serializationContext.StartScope($"case States.{node.Name}:");
            serializationContext.AppendLine($"{node.Name}();");
            serializationContext.AppendLine("break;");
        }
        using Scope terminateCaseScope = serializationContext.StartScope($"case States.Terminate:");
        serializationContext.AppendLine($"return;");
    }
}

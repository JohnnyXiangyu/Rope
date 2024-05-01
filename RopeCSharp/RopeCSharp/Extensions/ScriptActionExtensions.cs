﻿using RopeCSharp.Serialization;
using System.Text;
using Rope.Abstractions.Models;

namespace RopeCSharp.Extensions;
internal static class ScriptActionExtensions
{
    public static void Serialize(this ScriptAction self, SerializationContext context)
    {
        StringBuilder builder = new();
        builder.Append($"context.{self.Action}(");
        bool first = true;
        foreach (Value value in self.Values)
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

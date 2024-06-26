﻿using Rope.Abstractions.Reflection;
using System.Text;

namespace RopeCSharp.Serialization;
public class SerializationContext(Dictionary<string, ContextType> contextTypes, int indentation = 4)
{
    private int _depth = 0;
    private readonly StringBuilder _builder = new();

    public int Indentation { get => indentation; }
    public Dictionary<string, ContextType> ContextTypes { get => contextTypes; }
    public ContextType? SelectedContextType { get; set; }

    public Scope StartScope(string purpose = "")
    {
        AppendLine(purpose);
        AppendLine("{");
        _depth++;
        return new Scope(this, _depth);
    }

    public void ReleaseScope()
    {
        _depth--;
        AppendLine("}");
    }

    public void AppendLine(string text)
    {
        for (int i = 0; i < _depth * Indentation; i++)
        {
            _builder.Append(' ');
        }
        _builder.AppendLine(text.Trim());
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}
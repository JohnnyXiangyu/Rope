using Godot;
using System;
using System.Collections.Generic;

namespace RopeUI.Scripts.MediatorPattern;
public partial class DependencyManger : Node
{
    private readonly Dictionary<Type, Node> _services = [];

    // allow child nodes to opt in
    public override void _Ready()
    {
        foreach (IDependent child in RecursivelyGetDependents())
        {
            child.Configure(this);
        }
    }

    private IEnumerable<IDependent> RecursivelyGetDependents()
    {
        Queue<Node> pendingNodes = new(GetChildren());
        while (pendingNodes.Count > 0)
        {
            Node nextNode = pendingNodes.Dequeue();
            foreach (Node child in nextNode.GetChildren())
            {
                pendingNodes.Enqueue(child);
            }

            IDependent dependent;
            try
            {
                dependent = (IDependent)nextNode;
            }
            catch
            {
                continue;
            }
            yield return dependent;
        }
    }

    public bool AddSingleton<T>(T instance) where T : Node
    {
        return _services.TryAdd(typeof(T), instance);
    }

    public T? GetSingleton<T>() where T : Node
    {
        if (!_services.TryGetValue(typeof(T), out Node? value))
        {
            return null;
        }
        return (T)value;
    }

    public void TryRemoveSingleton<T>(T instance) where T : Node
    {
        if (_services.TryGetValue(typeof(T), out Node? value) && (T)value == instance)
        {
            _services.Remove(typeof(T));
        }
    }
}

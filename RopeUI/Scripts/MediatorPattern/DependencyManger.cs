using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RopeUI.Scripts.MediatorPattern;
public partial class DependencyManger : Node, IStopPluginSearch
{
    // runtime not modifiable
    private readonly Dictionary<Type, object> _singletonServices = [];
    private readonly Dictionary<Type, Func<DependencyManger, object>> _scopedServices = [];

    // runtime modifiable
    private readonly Dictionary<Type, object> _createdScopedServices = [];

    // allow child nodes to opt in and out
    public override void _Ready()
    {
        var allDependents = RecursivelyGetDependents().ToArray();
        foreach (IPlugin child in allDependents)
        {
            child.ConfigureServices(this);
        }
        foreach (IPlugin child in allDependents)
        {
            child.ContainerSetup(this);
        }
    }

    public override void _Process(double _)
    {
        _createdScopedServices.Clear();
    }

    private IEnumerable<IPlugin> RecursivelyGetDependents()
    {
        Queue<Node> pendingNodes = new(GetChildren());
        while (pendingNodes.Count > 0)
        {
            Node nextNode = pendingNodes.Dequeue();

            // don't get the children of a search stopper
            try
            {
                _ = (IStopPluginSearch)nextNode;
            }
            catch
            {
                foreach (Node child in nextNode.GetChildren())
                {
                    pendingNodes.Enqueue(child);
                }
            }

            // grab only nodes implementing IDependent
            IPlugin dependent;
            try
            {
                dependent = (IPlugin)nextNode;
            }
            catch
            {
                continue;
            }
            yield return dependent;
        }
    }

    public bool AddSingleton<T>(T instance) where T : class
    {
        return _singletonServices.TryAdd(typeof(T), instance);
    }

    public bool AddScoped<T>(Func<DependencyManger, T> factory) where T : class
    {
        return _scopedServices.TryAdd(typeof(T), factory);
    }

    public T? GetSingleton<T>() where T : class
    {
        try
        {
            if (!_singletonServices.TryGetValue(typeof(T), out object? value))
                return null;
            
            return (T)value;
        }
        catch
        {
            return null;
        }
    }

    public T? GetScoped<T>() where T : class
    {
        try
        {
            if (_createdScopedServices.TryGetValue(typeof(T), out object? value))
                return (T)value;
            
            if (!_scopedServices.TryGetValue(typeof(T), out Func<DependencyManger, object>? factory))
                return null;

            _createdScopedServices[typeof(T)] = factory.Invoke(this);
            return (T)_createdScopedServices[typeof(T)];
        }
        catch
        {
            return null;
        }
    }
}

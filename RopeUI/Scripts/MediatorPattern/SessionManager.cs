using Godot;
using Rope.Abstractions.Models;
using RopeUI.Common;

namespace RopeUI.Scripts.MediatorPattern;
public partial class SessionManager : Node, IPlugin
{
    private DependencyManger? _dependencyManger;

    public RopeScript? Script { get; set; }

    public QuickObservable<RopeScript> ScriptAccouncement { get; set; } = new();

    public void ConfigureServices(DependencyManger depdencyManager)
    {
        _dependencyManger = depdencyManager;
        if (!depdencyManager.AddSingleton(this))
        {
            QueueFree();
        }
    }

    public void ContainerSetup(DependencyManger dependencyManger) { }
}

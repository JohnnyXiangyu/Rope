using Godot;
using Rope.Abstractions.Models;
using RopeUI.Common;

namespace RopeUI.Scripts.MediatorPattern;
public partial class SessionManager : Node, IDependent
{
    private DependencyManger? _dependencyManger;

    public RopeScript? Script { get; set; }

    public QuickObservable<RopeScript> ScriptAccouncement { get; set; } = new();

    public void Configure(DependencyManger depdencyManager)
    {
        _dependencyManger = depdencyManager;
        if (!depdencyManager.AddSingleton(this))
        {
            QueueFree();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _dependencyManger?.TryRemoveSingleton(this);
    }
}

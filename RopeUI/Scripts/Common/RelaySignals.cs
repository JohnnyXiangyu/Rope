using Godot;

namespace RopeUI.Common;
public partial class RelaySignals : Node
{
    public void SelfDestruct()
    {
        QueueFree();
    }
}

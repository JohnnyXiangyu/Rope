using Godot;
using Rope.Abstractions.Reflection;

namespace RopeUI.Scripts.UserInterface.GraphNodes;

public partial class ActionNode : GraphNode
{
    public DataBase CascadedValue { get; set; }

    public string NodeName { get; set; }
}

using Godot;

namespace RopeUI.Scripts.UserInterface;

public partial class NewNodeButton : Button
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ButtonDown += ButtonPressedEvent;
    }


    private void ButtonPressedEvent()
    {
        PackedScene newNode = GD.Load<PackedScene>("res://UI/TestGraphNode.tscn");
        GraphEdit graphBody = GetNode<GraphEdit>("../../FuncitonalDocks/GraphEdit");
        newNode.Instantiate();
        graphBody.AddChild(newNode.Instantiate());
    }
}

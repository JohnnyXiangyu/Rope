using Godot;

namespace RopeUI.Scripts.UserInterface;

public partial class TransitionLabel : HBoxContainer
{
    [Export]
    public NodePath LabelChildPath { get; set; }
    private Label _labelChild;

    public int SelfIndex { get => int.Parse(_labelChild.Text); set => _labelChild.Text = value.ToString(); }

    public override void _Ready()
    {
        _labelChild = (Label)GetNode(LabelChildPath);
    }

    public void SelfDestruct()
    {
        QueueFree();
    }
}

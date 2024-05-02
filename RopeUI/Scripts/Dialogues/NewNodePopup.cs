using Godot;
using System;

namespace RopeUI.Scripts.Dialogues;

public partial class NewNodePopup : Node
{
    public string Input { get; set; }
    public Action<string> Callback { get; set; } = _ => { };

    [Signal]
    public delegate void NodeCreationConfirmEventHandler(string nodeName);

    [Export]
    public NodePath ButtonPath { get; set; }
    [Export]
    public NodePath InputBoxPath { get; set; }

    private Button ButtonChild;
    private TextEdit InputChild;

    public event Action<string> NodeCreation
    {
        add
        {
            Connect(SignalName.NodeCreationConfirm, Callable.From(value));
        }
        remove
        {
            Disconnect(SignalName.NodeCreationConfirm, Callable.From(value));
        }
    }

    public override void _Ready()
    {
        ButtonChild = (Button)GetNode(ButtonPath);
        InputChild = (TextEdit)GetNode(InputBoxPath);
    }

    public void OnCancel()
    {
        GD.Print($"node creation cancelled");
        QueueFree();
    }

    public void OnConfirm()
    {
        EmitSignal(SignalName.NodeCreationConfirm, InputChild.Text);
        GD.Print($"new node name: {InputChild.Text}");
        QueueFree();
    }
}

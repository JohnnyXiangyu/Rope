using Godot;
using System;

namespace RopeUI.Scripts.Dialogues;

public partial class NewNodePopup : Node
{
    public string Input { get; set; } = string.Empty;
    public Action<string> Callback { get; set; } = _ => { };

    [Signal]
    public delegate void NodeCreationConfirmEventHandler(string nodeName, NewNodePopup popup);

    [Export]
    public NodePath? ButtonPath { get; set; }
    private Button? ButtonChild;

    [Export]
    public NodePath? InputBoxPath { get; set; }
    private TextEdit? InputChild;

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
        if (InputChild!.Text.Length == 0)
            return;

        EmitSignal(SignalName.NodeCreationConfirm, InputChild!.Text, this);
    }
}

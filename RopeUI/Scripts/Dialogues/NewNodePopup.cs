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
    public Button? ButtonChild { get; set; }

    [Export]
    public LineEdit? InputChild { get; set; }  

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

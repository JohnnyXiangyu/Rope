using Godot;
using System;

namespace RopeUI.Scripts.Dialogues;

public partial class OpenFileDialog : FileDialog
{
    [Signal]
    public delegate void ProxiedFileSelectionEventHandler(Node sender, string path);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        FileSelected += OnFileSelected;
        Canceled += OnCancel;
    }

    private void OnFileSelected(string path)
    {
        EmitSignal(SignalName.ProxiedFileSelection, this, path);
    }

    private void OnCancel()
    {
        GD.Print("canceled file loading");
        QueueFree();
    }
}

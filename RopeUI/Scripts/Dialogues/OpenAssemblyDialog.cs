using Godot;
using System;

namespace RopeUI.Scripts.Dialogues;

public partial class OpenAssemblyDialog : FileDialog
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        FileSelected += OnAssemblySelected;
	}

    private void OnAssemblySelected(string path)
    {
        throw new NotImplementedException();
    }
}

using Godot;
using Rope.Abstractions.Reflection;

namespace RopeUI.Scripts.Dialogues;

public partial class NewActionPopup : Node
{
    [Signal]
    public delegate void ConfirmActionEventHandler(Node sender, string chosen);
    [Export]
    public OptionButton? OptionButtonChild { get; set; }

    public override void _Ready()
    {
        OptionButtonChild!.ItemSelected += Confirm;
    }

    public void DisplayOptions(ContextType contextType)
    {
        foreach (string actionType in contextType.Actions.Keys)
        {
            OptionButtonChild!.AddItem(actionType);
        }
        OptionButtonChild!.Select(-1);
    }

    public void Confirm(long index)
    {
        EmitSignal(SignalName.ConfirmAction, this, OptionButtonChild!.GetItemText((int)index));
    }

    public void OnCancel()
    {
        GD.Print("cancel action creation");
    }
}

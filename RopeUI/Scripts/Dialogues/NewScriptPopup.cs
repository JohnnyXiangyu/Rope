using Godot;
using Rope.Abstractions.Reflection;

namespace RopeUI.Scripts.Dialogues;
public partial class NewScriptPopup : Node
{
    [Export]
    public LineEdit? ScriptNameBox { get; set; }
    [Export]
    public LineEdit? NamespaceBox { get; set; }
    [Export]
    public OptionButton? OptionButtonChild { get; set; }

    [Signal]
    public delegate void NewScriptConfirmedEventHandler(Node sender, string newName, string newNamespace, string contextType);

    public void DisplayOptions(DataBase database)
    {
        foreach (string option in database.ContextTypes.Keys)
        {
            OptionButtonChild!.AddItem(option);
        }
        OptionButtonChild!.Select(0);
    }

    public void ConfirmNewNode()
    {
        if (ScriptNameBox!.Text.Length <= 0)
            return;
        EmitSignal(SignalName.NewScriptConfirmed, this, ScriptNameBox!.Text, NamespaceBox!.Text, OptionButtonChild!.GetItemText(OptionButtonChild.Selected));
    }
}

using Godot;

namespace RopeUI.Scripts.UserInterface.PrimitiveInput;
public partial class InputHasToBeInt32 : LineEdit
{
    private string _oldString = string.Empty;

    [Signal]
    public delegate void UpdateInt32ValueEventHandler(int value);

    public override void _Ready()
    {
        base._Ready();
        TextChanged += OnTextChange;
    }

    private void OnTextChange(string newText)
    {
        if (newText == string.Empty)
        {
            EmitSignal(SignalName.UpdateInt32Value, 0);
            _oldString = string.Empty;
            return;
        }

        try
        {
            EmitSignal(SignalName.UpdateInt32Value, int.Parse(newText));
            _oldString = newText;
        }
        catch
        {
            Text = _oldString;
        }
    }
}

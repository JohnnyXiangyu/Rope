using Godot;

namespace RopeUI.Scripts.UserInterface.PrimitiveInput;
public partial class InputHasToBeInt64 : LineEdit
{
    private string _oldString = string.Empty;

    [Signal]
    public delegate void UpdateInt64EventHandler(long value);

    public override void _Ready()
    {
        base._Ready();
        TextChanged += OnTextChange;
    }

    private void OnTextChange(string newText)
    {
        if (newText == string.Empty)
        {
            EmitSignal(SignalName.UpdateInt64, 0);
            _oldString = string.Empty;
            return;
        }

        try
        {
            EmitSignal(SignalName.UpdateInt64, long.Parse(newText));
            _oldString = newText;
        }
        catch
        {
            Text = _oldString;
        }
    }
}

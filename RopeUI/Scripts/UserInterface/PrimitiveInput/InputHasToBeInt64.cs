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
        try
        {
            _oldString = newText;
            EmitSignal(SignalName.UpdateInt64, long.Parse(_oldString));
        }
        catch
        {
            Text = _oldString;
        }
    }
}

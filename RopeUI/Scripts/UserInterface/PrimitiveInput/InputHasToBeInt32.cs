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
        try
        {
            _oldString = newText;
            EmitSignal(SignalName.UpdateInt32Value, int.Parse(_oldString));
        }
        catch
        {
            Text = _oldString;
        }
    }
}

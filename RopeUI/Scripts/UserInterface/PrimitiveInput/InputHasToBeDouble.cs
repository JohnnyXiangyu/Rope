using Godot;

namespace RopeUI.Scripts.UserInterface.PrimitiveInput;
public partial class InputHasToBeDouble : LineEdit
{
    private string _oldString = string.Empty;

    [Signal]
    public delegate void UpdateInputEventHandler(double newValue);

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
            EmitSignal(SignalName.UpdateInput, double.Parse(_oldString));
        }
        catch
        {
            Text = _oldString;
        }
    }
}

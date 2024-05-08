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
        if (newText == string.Empty)
        {
            EmitSignal(SignalName.UpdateInput, 0);
            _oldString = string.Empty;
            return;
        }

        try
        {
            EmitSignal(SignalName.UpdateInput, double.Parse(newText));
            _oldString = newText;
        }
        catch
        {
            Text = _oldString;
        }
    }
}

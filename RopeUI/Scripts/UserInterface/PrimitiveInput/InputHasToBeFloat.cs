using Godot;

namespace RopeUI.Scripts.UserInterface.PrimitiveInput;
public partial class InputHasToBeFloat : LineEdit
{
    private string _oldString = string.Empty;

    [Signal]
    public delegate void UpdateFloatValueEventHandler(float value);

    public override void _Ready()
    {
        base._Ready();
        TextChanged += OnTextChange;
    }

    private void OnTextChange(string newText)
    {
        if (newText == string.Empty)
        {
            EmitSignal(SignalName.UpdateFloatValue, 0);
            _oldString = string.Empty;
            return;
        }

        try
        {
            EmitSignal(SignalName.UpdateFloatValue, float.Parse(newText));
            _oldString = newText;
        }
        catch
        {
            Text = _oldString;
        }
    }
}

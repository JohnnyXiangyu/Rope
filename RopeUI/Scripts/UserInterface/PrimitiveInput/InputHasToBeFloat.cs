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
        try
        {
            _oldString = newText;
            EmitSignal(SignalName.UpdateFloatValue, float.Parse(_oldString));
        }
        catch
        {
            Text = _oldString;
        }
    }
}

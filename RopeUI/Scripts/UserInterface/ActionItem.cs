using Godot;
using Rope.Abstractions.Reflection;
using RopeUI.Scripts.UserInterface.PrimitiveInput;

namespace RopeUI.Scripts.UserInterface;

public partial class ActionItem : Node
{
    [Export]
    private PackedScene? StringInputPack;

    [Export]
    private PackedScene? Int32InputPack;

    [Export]
    private PackedScene? Int64InputPack;

    [Export]
    private PackedScene? BoolInputPack;

    [Export]
    private PackedScene? FloatInputPack;

    [Export]
    private PackedScene? DoubleInputPack;

    [Export]
    public PackedScene? ParamBoxPack { get; set; } = null;

    [Export]
    public Label? LabelChild { get; set; } = null;

    public ContextType? CurrentContext { get; set; } = null;

    public override void _Ready()
    {
        // get the children references
        if (LabelChild == null)
            throw new System.Exception("ActionItem is not initialized properly with reference to its label child.");
        if (ParamBoxPack == null)
            throw new System.Exception("ActionItem is not initialized properly with reference to a packed scene for InputBoxContainer.");
    }

    public void DisplayAction(Rope.Abstractions.Models.RopeAction data)
    {
        if (CurrentContext == null)
        {
            GD.PushWarning("warning: attemping to display action without loading reflection database");
            return;
        }

        ContextAction? action = CurrentContext.Actions[data.Action];
        if (action == null)
        {
            GD.PushError("error: attempting to display action with illegal name");
            return;
        }

        LabelChild!.Text = $"[ACTION] {action.Name}";

        // free previously displayed children
        foreach (Node child in GetChildren())
        {
            if (child == LabelChild)
                continue;
            child.QueueFree();
        }

        // fix data length mismatches
        while (action.Params.Length > data.Values.Count)
        {
            data.Values.Add(string.Empty);
        }
        while (action.Params.Length < data.Values.Count)
        {
            data.Values.RemoveAt(data.Values.Count - 1);
        }

        // fix data format mismatches
        for (int i = 0; i < action.Params.Length; i++)
        {
            try
            {
                switch (action.Params[i].Type)
                {
                    case LiteralType.Float:
                        float.Parse(data.Values[i]);
                        break;
                    case LiteralType.Double:
                        double.Parse(data.Values[i]);
                        break;
                    case LiteralType.Boolean:
                        bool.Parse(data.Values[i]);
                        break;
                    case LiteralType.Int64:
                        long.Parse(data.Values[i]);
                        break;
                    case LiteralType.Int32:
                        int.Parse(data.Values[i]);
                        break;
                    case LiteralType.String:
                        continue;
                }
            }
            catch
            {
                if (action.Params[i].Type == LiteralType.Boolean)
                {
                    data.Values[i] = "true";
                }
                else
                {
                    data.Values[i] = "0";
                }
            }
        }

        // display a new set of children for this action
        for (int i = 0; i < action.Params.Length; i++)
        {
            // make title card for parameter
            Parameter paramArchetype = action.Params[i];
            ParamInput paramBoxChild = (ParamInput)ParamBoxPack!.Instantiate();
            paramBoxChild.Display(paramArchetype.Name);
            AddChild(paramBoxChild);

            // make input box for parameter
            Node inputBoxChild;
            switch (action.Params[i].Type)
            {
                case LiteralType.Float:
                    {
                        int localIndex = i;
                        var child = (InputHasToBeFloat)FloatInputPack!.Instantiate();
                        child.Text = data.Values[localIndex];
                        inputBoxChild = child;
                        child.UpdateFloatValue += newVal => data.Values[localIndex] = newVal.ToString();
                    }
                    break;
                case LiteralType.Double:
                    {
                        int localIndex = i;
                        var child = (InputHasToBeDouble)DoubleInputPack!.Instantiate();
                        child.Text = data.Values[localIndex];
                        inputBoxChild = child;
                        child.UpdateInput += newVal => data.Values[localIndex] = newVal.ToString();
                    }
                    break;
                case LiteralType.Boolean:
                    {
                        int localIndex = i;
                        var child = (CheckBox)FloatInputPack!.Instantiate();
                        child.ButtonPressed = bool.Parse(data.Values[localIndex]);
                        inputBoxChild = child;
                        child.ButtonDown += () => data.Values[localIndex] = true.ToString();
                        child.ButtonUp += () => data.Values[localIndex] = false.ToString();
                    }
                    break;
                case LiteralType.Int64:
                    {
                        int localIndex = i;
                        var child = (InputHasToBeInt64)Int64InputPack!.Instantiate();
                        child.Text = data.Values[localIndex];
                        inputBoxChild = child;
                        child.UpdateInt64 += newVal => data.Values[localIndex] = newVal.ToString();
                    }
                    break;
                case LiteralType.Int32:
                    {
                        int localIndex = i;
                        var child = (InputHasToBeInt32)Int32InputPack!.Instantiate();
                        child.Text = data.Values[localIndex];
                        inputBoxChild = child;
                        child.UpdateInt32Value += newVal => data.Values[localIndex] = newVal.ToString();
                    }
                    break;
                case LiteralType.String:
                    {
                        int localIndex = i;
                        var child = (LineEdit)StringInputPack!.Instantiate();
                        child.Text = data.Values[localIndex];
                        inputBoxChild = child;
                        child.TextChanged += newVal => data.Values[localIndex] = newVal;
                    }
                    break;
                default:
                    throw new System.Exception($"unexpected type: {action.Params[i].Type}");
            }

            // add the input box to the title
            paramBoxChild.AddChild(inputBoxChild);
        }
    }
}

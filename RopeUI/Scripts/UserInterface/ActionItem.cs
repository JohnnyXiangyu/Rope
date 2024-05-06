using Godot;
using Rope.Abstractions.Reflection;
using RopeUI.Scripts.UserInterface.PrimitiveInput;
using System.Collections.Generic;
using System.Linq;

namespace RopeUI.Scripts.UserInterface;

public partial class ActionItem : VBoxContainer
{
    [Export]
    public string StringInputRes { get; set; } = string.Empty;
    private PackedScene? _stringInputPack;

    [Export]
    public string Int32InputRes { get; set; } = string.Empty;
    private PackedScene? _int32InputPack;

    [Export]
    public string Int64InputRes { get; set; } = string.Empty;
    private PackedScene? _int64InputPack;

    [Export]
    public string BoolInputRes { get; set; } = string.Empty;
    private PackedScene? _boolInputPack;

    [Export]
    public string FloatInputRes { get; set; } = string.Empty;
    private PackedScene? _floatInputPack;

    [Export]
    public string DoubleInputRes { get; set; } = string.Empty;
    private PackedScene? _doubleInputPack;

    [Export]
    public PackedScene? ParamBoxPack { get; set; } = null;

    [Export]
    public Label? LabelChild { get; set; } = null;

    public ContextType? CurrentContext { get; set; } = null;

    public override void _Ready()
    {
        // load all the input resources
        _int64InputPack = (PackedScene)GD.Load(Int64InputRes);
        _boolInputPack = (PackedScene)GD.Load(BoolInputRes);
        _floatInputPack = (PackedScene)GD.Load(FloatInputRes);
        _doubleInputPack = (PackedScene)GD.Load(DoubleInputRes);
        _int32InputPack = (PackedScene)GD.Load(Int32InputRes);
        _stringInputPack = (PackedScene)GD.Load(StringInputRes);

        // get the children references
        if (LabelChild == null)
            throw new System.Exception("ActionItem is not initialized properly with reference to its label child.");
        if (ParamBoxPack == null)
            throw new System.Exception("ActionItem is not initialize properly with reference to a packed scene for InputBoxContainer.");
    }

    public void DisplayAction(Rope.Abstractions.Models.ScriptAction data)
    {
        if (CurrentContext == null)
        {
            GD.PushWarning("warning: attemping to display action without loading reflection database");
            return;
        }

        ContextAction? action = CurrentContext.Actions.Where(a => a.Name == data.Action).FirstOrDefault();
        if (action == null)
        {
            GD.PushError("error: attempting to display action with illegal name");
            return;
        }

        LabelChild!.Text = action.Name;

        // free previously displayed children
        foreach (Node child in GetChildren())
        {
            if (child == LabelChild)
                continue;
            child.QueueFree();
        }

        // display a new set of children for this action
        for (int i = 0; i < action.Params.Length; i++)
        {
            Parameter param = action.Params[i];
            List<Node> inputBoxChildren = [];
            for (int j = 0; j < param.DataConstructor.Params.Length; j++)
            {
                LiteralType paramType = param.DataConstructor.Params[j];
                switch (paramType)
                {
                    case LiteralType.String:
                        {
                            LineEdit stringBox = (LineEdit)_stringInputPack!.Instantiate();
                            inputBoxChildren.Add(stringBox);
                            stringBox.Text = data.Values[i].Params[j];
                            stringBox.TextChanged += newString => data.Values[i].Params[j] = newString;
                            break;
                        }
                    case LiteralType.Float:
                        {
                            InputHasToBeDouble floatBox = (InputHasToBeDouble)_floatInputPack!.Instantiate();
                            inputBoxChildren.Add(floatBox);
                            floatBox.Text = data.Values[i].Params[j].ToString();
                            floatBox.UpdateInput += newFloat => data.Values[i].Params[j] = newFloat.ToString();
                            break;
                        }
                    case LiteralType.Double:
                        {
                            InputHasToBeDouble doubleBox = (InputHasToBeDouble)_doubleInputPack!.Instantiate();
                            inputBoxChildren.Add(doubleBox);
                            doubleBox.Text = data.Values[i].Params[j].ToString();
                            doubleBox.UpdateInput += newDouble => data.Values[i].Params[j] = newDouble.ToString();
                            break;
                        }
                    case LiteralType.Boolean:
                        {
                            CheckBox boolBox = (CheckBox)_boolInputPack!.Instantiate();
                            inputBoxChildren.Add(boolBox);
                            boolBox.ButtonPressed = bool.Parse(data.Values[i].Params[j]);
                            boolBox.ButtonDown += () => data.Values[i].Params[j] = true.ToString();
                            boolBox.ButtonUp += () => data.Values[i].Params[j] = false.ToString();
                            break;
                        }
                    case LiteralType.Int64:
                        {
                            InputHasToBeInt64 longBox = (InputHasToBeInt64)_int64InputPack!.Instantiate();
                            inputBoxChildren.Add(longBox);
                            longBox.Text = data.Values[i].Params[j];
                            longBox.UpdateInt64 += newInt64 => data.Values[i].Params[j] = newInt64.ToString();
                            break;
                        }
                    case LiteralType.Int32:
                        {
                            InputHasToBeInt32 intBox = (InputHasToBeInt32)_int32InputPack!.Instantiate();
                            inputBoxChildren.Add(intBox);
                            intBox.Text = data.Values[i].Params[j];
                            intBox.UpdateInt32Value += newInt32 => data.Values[i].Params[j] = newInt32.ToString();
                            break;
                        }
                    default:
                        {
                            throw new System.Exception("unexpected literal type encountered");
                        }
                }
            }

            ParamInput paramBoxChild = (ParamInput)ParamBoxPack!.Instantiate();
            paramBoxChild.Ready += () =>
            {
                paramBoxChild.Display(param.Name, inputBoxChildren);
            };
            AddChild(paramBoxChild);
        }
    }
}

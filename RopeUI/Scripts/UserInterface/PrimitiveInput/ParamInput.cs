using Godot;
using System;
using System.Collections.Generic;

namespace RopeUI.Scripts.UserInterface.PrimitiveInput;

public partial class ParamInput : Node
{
    [Export]
    public Label? LabelChild { get; set; }

    public void Display(string? paramName, IEnumerable<Node> paramBoxes)
    {
        if (LabelChild == null)
            throw new Exception("InputBoxContainer is not initialized with a reference to its label child.");

        LabelChild.Text = $"{paramName}:";

        foreach (Node node in paramBoxes)
        {
            AddChild(node);
        }
    }
}

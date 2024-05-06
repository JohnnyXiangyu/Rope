using Godot;
using RopeUI.Scripts.UserInterface.GraphNodes;
using System.Collections.Generic;
using System.Text;

namespace RopeUI.Scripts.UserInterface;

public partial class PropertyPanel : VBoxContainer
{
    [Export]
    public Label? LabelChild { get; set; } = null;

    [Export]
    public PackedScene? ActionBoxPack { get; set; } = null;

    private MainGraphEditor? _mainEditor;

    private ActionNode? _displayedActionNode = null;
    private Node? _currentDisplayPanel = null;

    private readonly List<Node> _displayedItems = [];

    public override void _Ready()
    {
        if (LabelChild == null)
            throw new System.Exception("LabelChild not set prior to instantiation");
        if (ActionBoxPack == null)
            throw new System.Exception("ActionBoxPack not set prior to instantiation");
    }

    public void SetMainEditor(MainGraphEditor editor)
    {
        _mainEditor = editor;
    }

    public void DisplayActionNode(ActionNode nodeToDisplay)
    {
        if (_mainEditor?.Context == null)
        {
            GD.PushError("trying to display ActionNode details without selecting context type first");
            return;
        }

        _displayedActionNode = nodeToDisplay;

        // display a node's name
        LabelChild!.Text = GetDescription(nodeToDisplay);

        // display all of its existing actions
        foreach (Rope.Abstractions.Models.ScriptAction action in nodeToDisplay.DataNode!.Actions)
        {
            ActionItem newAction = (ActionItem)ActionBoxPack!.Instantiate();
            newAction.CurrentContext = _mainEditor.Context;
            newAction.Ready += () =>
            {
                newAction.DisplayAction(action);
            };
        }
    }

    public void CancelDisplay(ActionNode nodeToStopDisplay)
    {
        if (_displayedActionNode != nodeToStopDisplay)
            return;

        foreach (Node item in _displayedItems)
        {
            RemoveChild(item);
            item.QueueFree();
        }

        LabelChild!.Text = string.Empty;
    }

    private static string GetDescription(ActionNode node)
    {
        StringBuilder builder = new();
        builder.AppendLine("Type: ActionNode");
        builder.AppendLine($"Name: {node.ActionName}");
        return builder.ToString();
    }
}

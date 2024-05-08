using Godot;
using Rope.Abstractions.Models;
using RopeUI.Scripts.Dialogues;
using RopeUI.Scripts.MediatorPattern;
using RopeUI.Scripts.UserInterface.GraphNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace RopeUI.Scripts.UserInterface;

public partial class PropertyPanel : VBoxContainer, IPlugin
{
    [Export]
    public Label? LabelChild { get; set; } = null;

    [Export]
    public PackedScene? ActionBoxPack { get; set; } = null;

    [Export]
    public PackedScene? ActionCreationPack {  get; set; } = null;

    [Export]
    public Control[] PerActionChildren { get; set; } = [];
    private readonly HashSet<Node> _persistentChildren = [];

    private MainGraphEditor? _mainEditor;

    private ActionNode? _displayedActionNode = null;
    private Node? _currentDisplayPanel = null;

    private readonly List<IDisposable> _subscriptions = [];

    public override void _Ready()
    {
        if (LabelChild == null)
            throw new System.Exception("LabelChild not set prior to instantiation");
        if (ActionBoxPack == null)
            throw new System.Exception("ActionBoxPack not set prior to instantiation");
        if (ActionCreationPack == null)
            throw new System.Exception("ActionCreationPack not set prior to instantiation");

        foreach (Control control in PerActionChildren)
        {
            _persistentChildren.Add(control);
        }
    }

    public void DisplayActionNode(ActionNode? nodeToDisplay)
    {
        if (nodeToDisplay == null)
            return;

        foreach (Control child in PerActionChildren)
        {
            child.Visible = true;
        }

        if (_mainEditor?.Context == null)
        {
            GD.PushError("trying to display ActionNode details without selecting context type first");
            return;
        }

        _displayedActionNode = nodeToDisplay;

        // display a node's name
        LabelChild!.Text = GetDescription(nodeToDisplay);

        // display all of its existing actions
        GD.Print(nodeToDisplay.DataNode!.Actions.Count);
        foreach (RopeAction action in nodeToDisplay.DataNode!.Actions)
        {
            AddAction(action);
        }
    }

    private void AddAction(RopeAction action)
    {
        ActionItem newAction = (ActionItem)ActionBoxPack!.Instantiate();
        newAction.CurrentContext = _mainEditor!.Context;
        newAction.DisplayAction(action);
        AddChild(newAction);
    }

    public void CancelDisplay(ActionNode? nodeToStopDisplay)
    {
        if (_displayedActionNode != nodeToStopDisplay)
            return;

        foreach (Control child in PerActionChildren)
        {
            child.Visible = false;
        }

        foreach (Node child in GetChildren())
        {
            if (!_persistentChildren.Contains(child))
            {
                child.QueueFree();
            }
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

    public void OnAddAction()
    {
        var popup = (NewActionPopup)ActionCreationPack!.Instantiate();
        AddChild(popup);
        popup.DisplayOptions(_mainEditor!.Context!);
        popup.ConfirmAction += (_, actionType) =>
        {
            RopeAction action = new()
            {
                Action = actionType,
                Values = []
            };
            _displayedActionNode!.DataNode!.Actions.Add(action);
            AddAction(action);
            popup.QueueFree();
        };
    }

    public void ConfigureServices(DependencyManger depdencyManager) { }

    public void ContainerSetup(DependencyManger dependencyManger)
    {
        _mainEditor = dependencyManger.GetSingleton<MainGraphEditor>();
        if (_mainEditor == null)
        {
            GD.PushError($"property panel: required service MainGraphEditor not found");
            QueueFree();
            return;
        }

        _subscriptions.Add(_mainEditor.ActionNodeSelectionEvent.Subscribe(DisplayActionNode));
        _subscriptions.Add(_mainEditor.ActionNodeDeselectionEvent.Subscribe(CancelDisplay));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (IDisposable sub in _subscriptions)
        {
            sub.Dispose();
        }
    }
}

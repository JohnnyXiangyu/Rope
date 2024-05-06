using Godot;
using RopeUI.DataHolds;
using System.Collections.Generic;

namespace RopeUI.Scripts.UserInterface.GraphNodes;

public partial class ActionNode : GraphNode, IDataNodeHolder
{
    public static int FirstSlotOffset = 2;
    public static int InSlotOffset = 1;

    [Export]
    public PackedScene? TransitionLabelPack;

    public string ActionName { get; set; } = string.Empty;
    public string ConditionName { get; set; } = string.Empty;
    public Rope.Abstractions.Models.RopeNode? DataNode { get; set; }

    public List<TransitionLabel> TransitionLabels { get; private set; } = [];

    [Signal]
    public delegate void SlotRemovedEventHandler(string graphNodeName, int slot);

    public override void _Ready()
    {
        base._Ready();
        Title = ActionName;
        DataNode ??= new() { Actions = [], Name = ActionName, PosX = Position.X, PosY = Position.Y };
    }

    public void AddTransition()
    {
        TransitionLabel newLabel = (TransitionLabel)TransitionLabelPack!.Instantiate();
        AddChild(newLabel);
        ChildExitingTree += OnLabelExit;

        newLabel.SelfIndex = TransitionLabels.Count;
        TransitionLabels.Add(newLabel);
        SetSlot(TransitionLabels.Count + 1, false, 0, Color.Color8(255, 255, 255), true, 0, Color.Color8(255, 255, 255));

        // update branches to reserve some more slots
        while (DataNode!.Branches.Count < TransitionLabels.Count)
        {
            DataNode!.Branches.Add(string.Empty);
        }
    }

    private void OnLabelExit(Node node)
    {
        // figure out which node exited
        int index = TransitionLabels.IndexOf((TransitionLabel)node);

        // update and rename all child labels
        TransitionLabels.Remove((TransitionLabel)node);
        for (int i = 0; i <  TransitionLabels.Count; i++)
        {
            TransitionLabels[i].SelfIndex = i;
        }

        // signal
        EmitSignal(SignalName.SlotRemoved, Name, index);
    }
}

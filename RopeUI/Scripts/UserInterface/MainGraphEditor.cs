using Godot;
using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;
using RopeCSharp;
using RopeUI.Scripts.Dialogues;
using RopeUI.Scripts.UserInterface.GraphNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace RopeUI.Scripts.UserInterface;

public partial class MainGraphEditor : GraphEdit
{
    [Export]
    public PackedScene? NodeCreatePopupPack;
    [Export]
    public PackedScene? ActionNodePack;
    [Export]
    public PackedScene? LoadAssemblyPopupPack;
    [Export]
    public PackedScene? LoadScriptPopupPack;
    
    public DataBase? Database { get; private set; }
    public ContextType? Context { get; private set; }
    public RopeScript? Script { get; set; }

    [Signal]
    public delegate void ActionNodeSelectedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeletedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeselectedEventHandler(ActionNode node);
    [Signal]
    public delegate void AnnounceMainEditorEventHandler(MainGraphEditor editorItself);

    /// <summary>
    /// From graph node name to their connections (not action name)
    /// </summary>
    private readonly Dictionary<string, Dictionary<int, (string, int)>> _knownConnections = [];

    /// <summary>
    /// From action name to action node (not node name or path)
    /// </summary>
    private readonly Dictionary<string, ActionNode> _knownActionNodes = [];

    public override void _Ready()
    {
        ConnectionRequest += ConnectNodeWithRecord;
        NodeSelected += NodeSelectedDispatch;
        NodeDeselected += NodeDeselectedDispatch;
        Database = Service.LoadAssembly(Assembly.GetAssembly(AssemblyConfigs.AssemblyEntry) ?? throw new Exception("unable to load client assembly"));
        EmitSignal(SignalName.AnnounceMainEditor, this);
    }

    public void InitiateLoadScript()
    {
        if (Database == null || Script != null)
            return;

        OpenFileDialog scriptDialogue = (OpenFileDialog)LoadScriptPopupPack!.Instantiate();
        scriptDialogue.ProxiedFileSelection += (Node sender, string path) =>
        {
            try
            {
                using FileStream scriptFile = File.OpenRead(path);
                Script = JsonSerializer.Deserialize<RopeScript>(scriptFile);
                if (Script == null)
                {
                    throw new Exception();
                }
                SanityCheckScript(Script);

                // load the context type
                Context = Database.ContextTypes[Script.Context];

                // display all the nodes we have currently
                foreach (RopeNode scriptNode in Script.Nodes)
                {
                    CreateNodeInternal(scriptNode.Name, new Vector2(scriptNode.PosX, scriptNode.PosY), scriptNode);
                }

                // connect nodes
                foreach ((string name, ActionNode node) in _knownActionNodes)
                {
                    node.Ready += () =>
                    {
                        GD.PushError(name);
                    };
                    RopeNode? scriptNode = node.DataNode;
                    if (scriptNode?.HasValidTransition != true)
                        continue;

                    for (int i = 0; i < scriptNode.Branches.Count; i++)
                    {
                        if (_knownActionNodes.TryGetValue(scriptNode.Branches[i], out var targetNode))
                        {
                            ConnectNodeWithRecord(node.Name, i, targetNode.Name, 0);
                        }
                        else
                        {
                            scriptNode.Branches[i] = string.Empty;
                        }
                    }
                }

                // close popup
                sender.QueueFree();
                Visible = true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"unable to load script from {path}: {e}");
                Script = null;
                Context = null;
                Visible = false;
            }
        };
        AddSibling(scriptDialogue);
    }

    // designed to throw whenever something is wrong
    private void SanityCheckScript(RopeScript script)
    {
        // check context
        ContextType contextData = Database!.ContextTypes[script.Context];

        // check actions
        foreach (RopeNode node in script.Nodes)
        {
            foreach (RopeAction action in node.Actions)
            {
                _ = contextData.Actions[action.Action];
            }
        }
    }

    private void NodeDeselectedDispatch(Node node)
    {
        try
        {
            EmitSignal(SignalName.ActionNodeDeselected, (ActionNode)node);
        }
        catch { }
    }

    private void NodeSelectedDispatch(Node node)
    {
        try
        {
            EmitSignal(SignalName.ActionNodeSelected, (ActionNode)node);
        }
        catch { }
    }

    private void VerifyGraphNode(StringName nodeName)
    {
        _knownConnections.TryAdd(nodeName, []);
    }

    private void ConnectNodeWithRecord(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        GD.Print($"{fromNode}:{fromPort} -> {toNode}:{toPort}");
        VerifyGraphNode(fromNode);
        if (_knownConnections[fromNode].TryGetValue((int)fromPort, out var connection))
        {
            DisconnectNodeWithRecord(fromNode, (int)fromPort, connection.Item1, connection.Item2);
        }
        ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
        _knownConnections[fromNode][(int)fromPort] = (toNode, (int)toPort);

        // update model
        ActionNode actionNodeFrom = GetNode<ActionNode>($"./{fromNode}");
        ActionNode actionNodeTo = GetNode<ActionNode>($"./{toNode}");

        // NOTE: the logic for *creating* a transition structure is handled in the ActionNode itself, we assume it's created correctly here
        actionNodeFrom.DataNode!.Branches[(int)fromPort] = actionNodeTo.ActionName;
    }

    private void DisconnectNodeWithRecord(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        if (!_knownConnections.TryGetValue(fromNode, out var connection) 
            || !connection.TryGetValue((int)fromPort, out var target)
            || target.Item1 != toNode
            || target.Item2 != (int)toPort)
        {
            return;
        }

        GD.Print($"{fromNode}:{fromPort} -x-> {toNode}:{toPort}");
        VerifyGraphNode(fromNode);

        // disconnect
        DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);

        // update graph data
        _knownConnections[fromNode].Remove((int)fromPort);

        // update node data
        ActionNode fromNodeActionNode = GetNode<ActionNode>($"./{fromNode}");
        fromNodeActionNode.DataNode!.Branches[(int)fromPort] = string.Empty;
    }

    public void TryCreateNode()
    {
        if (!Visible)
            return;

        var newPopup = (NewNodePopup) NodeCreatePopupPack!.Instantiate();
        AddSibling(newPopup);
        newPopup.NodeCreationConfirm += (actionName, popup) =>
        {
            if (_knownActionNodes.ContainsKey(actionName))
            {
                GD.Print($"duplicate node creation attempt: {actionName}");
                return;
            }

            ActionNode newNode = CreateNodeInternal(actionName);

            GD.Print($"new action node: {actionName}");
            Script!.Nodes.Add(newNode.DataNode!);
            popup.QueueFree();
        };
    }

    public ActionNode CreateNodeInternal(string actionName, Vector2? initialPosition = null, RopeNode? initialDataNode = null)
    {
        var newNode = (ActionNode)ActionNodePack!.Instantiate();
        newNode.ActionName = actionName;
        newNode.PositionOffset = (initialPosition ?? Size / 2 + 2 * ScrollOffset) - ScrollOffset;
        newNode.SlotRemoved += OnSlotRemovedOnNode;
        newNode.DataNode = initialDataNode ?? new RopeNode() { Name = actionName, Actions = [], PosX = newNode.Position.X, PosY = newNode.Position.Y };
        AddChild(newNode);

        newNode.PositionOffsetChanged += () =>
        {
            newNode.DataNode.PosX = newNode.PositionOffset.X + ScrollOffset.X;
            newNode.DataNode.PosY = newNode.PositionOffset.Y + ScrollOffset.Y;
        };

        if (initialDataNode?.HasValidTransition == true)
        {
            foreach (var branch in initialDataNode.Branches)
            {
                newNode.AddTransition();
            }
        }

        _knownActionNodes.Add(actionName, newNode);
        return newNode;
    }

    private void OnSlotRemovedOnNode(string graphNodeName, int removedSlot)
    {
        GD.Print($"slot {removedSlot} removed on {graphNodeName}, shifting everything forwards");
        VerifyGraphNode(graphNodeName);
        int[] existingConnectedSlots = [.. _knownConnections[graphNodeName].Keys];
        foreach (int outSlot in existingConnectedSlots)
        {
            if (outSlot < removedSlot)
                continue;

            // disconnect and connect
            if (_knownConnections[graphNodeName].TryGetValue(outSlot, out (string, int) existingConnection))
            {
                DisconnectNode(graphNodeName, outSlot, existingConnection.Item1, existingConnection.Item2);
                _knownConnections[graphNodeName].Remove(outSlot);

                if (outSlot > removedSlot)
                {
                    ConnectNode(graphNodeName, outSlot - 1, existingConnection.Item1, existingConnection.Item2);
                    _knownConnections[graphNodeName][outSlot - 1] = existingConnection;
                }
            }
        }
    }

    public void DebugOutputScript()
    {
        if (!Visible)
            return;
        
        GD.Print("---- DEBUG OUTPUT: code ----");

        string output = Service.SerializeScript(Script!, Database!.ContextTypes);
        GD.Print(output);

        GD.Print("---- DEBUG OUTPUT FINISH ----");
    }

    public void DebugOutputJson()
    {
        if (!Visible)
            return;

        GD.Print("---- DEBUG OUTPUT: json ----");

        string output = JsonSerializer.Serialize(Script);
        GD.Print(output);

        GD.Print("---- DEBUG OUTPUT FINISH ----");
    }
}

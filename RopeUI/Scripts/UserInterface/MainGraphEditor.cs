using FirstPlayable.Abstractions;
using Godot;
using Rope.Abstractions.Reflection;
using RopeCSharp;
using RopeUI.Scripts.Dialogues;
using RopeUI.Scripts.UserInterface.GraphNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public Rope.Abstractions.Models.Script? Script { get; set; }

    [Signal]
    public delegate void ActionNodeSelectedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeletedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeselectedEventHandler(ActionNode node);
    [Signal]
    public delegate void AnnounceMainEditorEventHandler(MainGraphEditor editorItself);

    private readonly Dictionary<string, Dictionary<int, (string, int)>> _knownConnections = [];
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
                Script = JsonSerializer.Deserialize<Rope.Abstractions.Models.Script>(scriptFile);
                if (Script == null)
                {
                    throw new Exception();
                }

                sender.QueueFree();

                
                GD.Print(JsonSerializer.Serialize(Script));

                // load the context type
                Context = Database.ContextTypes[Script.Context];

                // display all the nodes we have currently
                foreach (Rope.Abstractions.Models.ScriptNode scriptNode in Script.Nodes)
                {
                    CreateNodeInternal(scriptNode.Name, new Vector2(scriptNode.PosX, scriptNode.PosY), scriptNode);
                }

                // TODO: connect nodes
            }
            catch (Exception e)
            {
                GD.PrintErr($"unable to load script from {path}: {e}");
            }
        };
        AddSibling(scriptDialogue);
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
            DisconnectNode(fromNode, (int)fromPort, connection.Item1, connection.Item2);
            _knownConnections[fromNode].Remove((int)fromPort);
        }
        ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
        _knownConnections[fromNode][(int)fromPort] = (toNode, (int)toPort);
    }

    public void TryCreateNode()
    {
        var newPopup = (NewNodePopup) NodeCreatePopupPack!.Instantiate();
        AddSibling(newPopup);
        newPopup.NodeCreationConfirm += (actionName, popup) =>
        {
            if (_knownActionNodes.ContainsKey(actionName))
            {
                GD.Print($"duplicate node creation attempt: {actionName}");
                return;
            }

            CreateNodeInternal(actionName);

            GD.Print($"new action node: {actionName}");
            popup.QueueFree();
        };
    }

    public void CreateNodeInternal(string actionName, Vector2? initialPosition = null, Rope.Abstractions.Models.ScriptNode? initialDataNode = null)
    {
        var newNode = (ActionNode)ActionNodePack!.Instantiate();
        newNode.DataNode = initialDataNode;
        newNode.ActionName = actionName;
        newNode.PositionOffset = initialPosition ?? Size / 2 + ScrollOffset;
        newNode.SlotRemoved += OnSlotRemovedOnNode;
        AddChild(newNode);

        _knownActionNodes.Add(actionName, newNode);
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
}

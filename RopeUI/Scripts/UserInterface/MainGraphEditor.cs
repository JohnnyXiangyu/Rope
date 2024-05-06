using Godot;
using Rope.Abstractions.Models;
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
    public RopeScript? Script { get; set; }

    [Signal]
    public delegate void ActionNodeSelectedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeletedEventHandler(ActionNode node);
    [Signal]
    public delegate void ActionNodeDeselectedEventHandler(ActionNode node);
    [Signal]
    public delegate void AnnounceMainEditorEventHandler(MainGraphEditor editorItself);

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
                ContextAction actionArchetype = contextData.Actions.Where(a => a.Name == action.Action).First();
                for (int i = 0; i < actionArchetype.Params.Length; i++)
                {
                    RopeValue value = action.Values[i];
                    if (value.Params.Length != actionArchetype.Params[i].DataConstructor.Params.Length)
                    {
                        // reset this value
                        value.Params = actionArchetype.Params[i].DataConstructor.Params.Select(_ => string.Empty).ToArray();
                    }
                }
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
            DisconnectNode(fromNode, (int)fromPort, connection.Item1, connection.Item2);
            _knownConnections[fromNode].Remove((int)fromPort);
        }
        ConnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
        _knownConnections[fromNode][(int)fromPort] = (toNode, (int)toPort);

        // update model
        ActionNode actionNodeFrom = GetNode<ActionNode>($"{GetPath()}/{fromNode}");
        ActionNode actionNodeTo = GetNode<ActionNode>($"{GetPath()}/{toNode}");

        // NOTE: the logic for *creating* a transition structure is handled in the ActionNode itself, we assume it's created correctly here
        actionNodeFrom.DataNode!.Branches[(int)fromPort] = actionNodeTo.ActionName;
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

            CreateNodeInternal(actionName);

            GD.Print($"new action node: {actionName}");
            popup.QueueFree();
        };
    }

    public void CreateNodeInternal(string actionName, Vector2? initialPosition = null, RopeNode? initialDataNode = null)
    {
        var newNode = (ActionNode)ActionNodePack!.Instantiate();
        newNode.DataNode = initialDataNode;
        newNode.ActionName = actionName;
        newNode.PositionOffset = initialPosition ?? Size / 2 + ScrollOffset;
        newNode.SlotRemoved += OnSlotRemovedOnNode;
        AddChild(newNode);

        if (initialDataNode?.HasValidTransition == true)
        {
            foreach (var branch in initialDataNode.Branches)
            {
                newNode.AddTransition();
            }
        }

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

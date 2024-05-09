using Godot;
using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;
using RopeUI.Scripts.Dialogues;
using RopeUI.Scripts.MediatorPattern;
using RopeUI.Scripts.UserInterface.GraphNodes;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace RopeUI.Scripts.UserInterface;

public partial class MainGraphEditor : GraphEdit, IPlugin
{
    [Export]
    public PackedScene? NodeCreatePopupPack;
    [Export]
    public PackedScene? ActionNodePack;

    public ContextType? Context { get; private set; }

    public BehaviorSubject<ActionNode?> ActionNodeSelectionEvent { get; private set; } = new(null);
    public BehaviorSubject<ActionNode?> ActionNodeDeselectionEvent { get; private set; } = new(null);

    /// <summary>
    /// From graph node name to their connections (not action name)
    /// </summary>
    private readonly Dictionary<string, Dictionary<int, (string, int)>> _knownConnections = [];

    /// <summary>
    /// From action name to action node (not node name or path)
    /// </summary>
    private readonly Dictionary<string, ActionNode> _knownActionNodes = [];

    private SessionManager? _sessionManager;

    private readonly List<IDisposable> _subscriptions = [];

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (IDisposable subscription in _subscriptions)
        {
            subscription.Dispose();
        }
    }

    public void ConfigureServices(DependencyManger dependencyManager)
    {
        if (!dependencyManager.AddSingleton(this))
        {
            GD.Print("Main editor failed to register self as singleton. Queuing free.");
            QueueFree();
            return;
        }
    }

    public void ContainerSetup(DependencyManger dependencyManger)
    {
        // request a session manager
        _sessionManager = dependencyManger.GetSingleton<SessionManager>();
        if (_sessionManager == null)
        {
            GD.Print("Main editor failed to acquire needed service: SessionManager. Queuing free.");
            QueueFree();
            return;
        }

        // subscribe to session manager events
        _subscriptions.Add(_sessionManager.ScriptAccouncement.Subscribe(newScript =>
        {
            if (newScript == null)
            {
                GD.Print($"main graph editor: unload script");
                UnloadScript();
                return;
            }

            GD.Print($"main graph editor: load new script {newScript.Name}");
            LoadScript(newScript);
        }));

        // hook self into the session manager
        ConnectionRequest += ConnectNodeWithRecord;
        NodeSelected += NodeSelectedDispatch;
        NodeDeselected += NodeDeselectedDispatch;
    }

    private void UnloadScript()
    {
        ClearConnections();
        foreach (Node child in GetChildren())
        {
            try
            {
                ((GraphNode)child).QueueFree();
            }
            catch { }
        }
        _knownActionNodes.Clear();
        _knownConnections.Clear();
    }

    private void LoadScript(RopeScript script)
    {
        // find the context
        Context = _sessionManager!.Database!.ContextTypes[script.Context];

        // display all the nodes we have currently
        foreach (RopeNode scriptNode in script.Nodes)
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
    }

    private void NodeDeselectedDispatch(Node node)
    {
        try
        {
            ActionNodeDeselectionEvent.OnNext((ActionNode)node);
        }
        catch { }
    }

    private void NodeSelectedDispatch(Node node)
    {
        try
        {
            ActionNodeSelectionEvent.OnNext((ActionNode)node);
        }
        catch { }
    }

    private void VerifyGraphNode(StringName nodeName)
    {
        _knownConnections.TryAdd(nodeName, []);
    }

    private void ConnectNodeWithRecord(StringName fromNode, long fromPort, StringName toNode, long toPort)
    {
        // update per graph metadata
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
        if (_sessionManager?.ScriptAccouncement?.Value == null)
            return;

        var newPopup = (NewNodePopup)NodeCreatePopupPack!.Instantiate();
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
            _sessionManager.ScriptAccouncement.Value.Nodes.Add(newNode.DataNode!);
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
}

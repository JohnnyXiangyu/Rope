using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RopeUI.Scripts.UserInterface;

public partial class MainGraphEditor : GraphEdit
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EndNodeMove += DebugNodeMove;
    }

    private void DebugNodeMove()
    {
        GD.Print(SerializeNode());
    }

    // TODO: follow up, position = positionoffset + scroll offset
    public string SerializeNode()
    {
        IEnumerable<string> childrenInformation = GetChildren()
            .SelectMany<Node, GraphNode>(child =>
            {
                try
                {
                    GraphNode graphNode = (GraphNode)child;
                    return [graphNode];
                }
                catch
                {
                    return [];
                }
            })
            .Select(child => $"{child.PositionOffset}, orig: {child.Position}");

        StringBuilder sb = new();
        foreach (string childInfo in childrenInformation)
        {
            sb.AppendLine(childInfo);
        }

        sb.AppendLine(ScrollOffset.ToString());

        return sb.ToString();
    }
}

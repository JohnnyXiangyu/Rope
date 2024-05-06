using System.Text.Json.Serialization;

namespace Rope.Abstractions.Models;
public class RopeNode
{
    public required string Name { get; set; }
    public required float PosX { get; set; }
    public required float PosY { get; set; }
    public required RopeAction[] Actions { get; set; }
    public string Condition { get; set; } = string.Empty;
    public List<string> Branches { get; set; } = [];

    [JsonIgnore]
    public bool HasValidTransition { get => Condition != string.Empty && Branches.Count > 0; }
}

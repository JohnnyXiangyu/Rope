using Rope.Abstractions.CSharpAttributes;

namespace FirstPlayable.Abstractions;

[ContextType]
public interface IContext
{
    [ConditionProperty]
    int Choice { get; }

    [ContextAction]
    void Dialogue(string name, string line, string verb = "says");
    [ContextAction]
    void GetPoints(int numPoints, string reason);
    [ContextAction]
    void Branch(params string[] choices);
}

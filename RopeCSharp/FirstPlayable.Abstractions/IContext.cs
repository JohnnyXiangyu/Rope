namespace FirstPlayable.Abstractions;
public interface IContext
{
    int Choice { get; }

    void Dialogue(string name, string line, string verb = "says");
    void GetPoints(int numPoints, string reason);
    void Branch(params string[] choices);
}

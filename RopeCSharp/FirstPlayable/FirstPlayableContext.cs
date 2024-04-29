using FirstPlayable.Abstractions;

namespace FirstPlayable;
internal class FirstPlayableContext : IContext
{
    public void GetPoints(int numPoints, string reason)
    {
        Console.Write($"You gained {numPoints} points for {reason}.");
        Console.ReadLine();
    }

    void IContext.Dialogue(string name, string line, string verb)
    {
        Console.Write($"{name} {verb}, \"{line}\"");
        Console.ReadLine();
    }
}

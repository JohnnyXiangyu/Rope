using FirstPlayable.Abstractions;

namespace FirstPlayable;
internal class FirstPlayableContext : IContext
{
    public int Choice { get; private set; }

    public void Branch(params string[] choices)
    {
        Console.WriteLine("Your response:");
        for (int i = 0; i < choices.Length; i ++)
        {
            Console.WriteLine($"{i} {choices[i]}");
        }
        string response = Console.ReadLine() ?? throw new ArgumentException("invalid input");
        Choice = int.Parse(response);
    }

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

using FirstPlayable.Abstractions;

namespace FirstPlayable;
internal class FirstPlayableContext : IContext
{
    void IContext.Dialogue(string name, string line, string verb)
    {
        Console.WriteLine($"{name} {verb}, \"{line}\"");
        Console.ReadLine();
    }
}

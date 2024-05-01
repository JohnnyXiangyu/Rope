namespace Playground;

internal class Program
{
    static void Main(string[] args)
    {
        string path = "C:\\Repos\\Rope\\RopeCSharp\\FirstPlayable.Abstractions\\bin\\Debug\\net8.0\\FirstPlayable.Abstractions.dll";
        var result = RopeCSharp.Service.LoadAssembly(path);
        Console.WriteLine(result);
    }
}

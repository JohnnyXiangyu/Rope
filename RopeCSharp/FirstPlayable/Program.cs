namespace FirstPlayable;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("First playable loaded.");

        FirstPlayableContext context = new();
        RopeCSharp.Client.Scripts.DemoScene demoScene = new(context);
        demoScene.Run();
    }
}

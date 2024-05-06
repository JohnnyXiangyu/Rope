using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;
using RopeCSharp;
using System.Text.Json;

namespace RopeCLI;

internal class Program
{
    static void Main(string[] args)
    {
        DataBase db = Service.LoadAssembly("C:\\Repos\\Rope\\RopeCSharp\\FirstPlayable.Abstractions\\bin\\Debug\\net8.0\\FirstPlayable.Abstractions.dll");
        const string filePath = "C:\\Repos\\Rope\\RopeCSharp\\FirstPlayable\\Scripts\\RoutineOne.rope.json";
        using FileStream fs = File.OpenRead(filePath);
        RopeScript? script = JsonSerializer.Deserialize<RopeScript>(fs);

        var something = db.ContextTypes[script.Context];

        Console.WriteLine("done");
    }
}

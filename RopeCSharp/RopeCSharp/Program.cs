using RopeCSharp.Models;
using RopeCSharp.Serialization;
using System.Reflection;

namespace RopeCSharp;

internal partial class Program
{
    static async Task Main(string[] _)
    {
        const string ScriptPath = "C:\\Repos\\Rope\\RopeCSharp\\FirstPlayable\\Scripts";

        // load the abstraction
        Dictionary<string, Type> contextTypes = Assembly.GetAssembly(typeof(FirstPlayable.Abstractions.IContext))!.GetTypes()
                              .Where(t => t.Namespace == "FirstPlayable.Abstractions")
                              .Aggregate(new Dictionary<string, Type>(), (dict, type) =>
                              {
                                  dict[type.Name] = type;
                                  return dict;
                              });

        // load scripts
        IEnumerable<string> scripts = Directory.EnumerateFiles(ScriptPath).Where(x => x.EndsWith(".rope.json"));
        foreach (string script in scripts)
        {
            using FileStream scriptFile = File.OpenRead(script);
            Script scriptObj = await Script.DeserializeAsync(script).ConfigureAwait(false);

            SerializationContext serializationContext = new(contextTypes);
            scriptObj.Serialize(serializationContext);

            // output file
            string outputScript = Path.Combine(ScriptPath, script.Replace(".rope.json", ".cs"));
            if (File.Exists(outputScript))
            {
                File.Delete(outputScript);
            }
            using StreamWriter outputWriter = new(outputScript);
            await outputWriter.WriteAsync(serializationContext.ToString()).ConfigureAwait(false);

            // debug view
            Console.WriteLine(serializationContext.ToString());
        }
    }
}

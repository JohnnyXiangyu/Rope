using RopeCSharp.Extensions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RopeCSharp;

internal partial class Program
{
    [GeneratedRegex("(?<actionName>[^-]+)-(?<values>.+)")]
    private static partial Regex ActionLineRegex();

    private record ScriptAction(string Action, IEnumerable<string> Values);

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
        IEnumerable<string> scripts = Directory.EnumerateFiles(ScriptPath).Where(x => x.EndsWith(".rope"));
        foreach (string script in scripts)
        {
            Type? contextType = null;
            List<ScriptAction> actions = [];

            using StreamReader scriptFs = new(script);
            

            // process each line
            await foreach (string line in scriptFs.ReadAllLinesAsync().ConfigureAwait(false))
            {
                if (line.Trim().Length == 0)
                    continue;

                // control statement
                if (line.StartsWith("use context "))
                {
                    string contextName = line.Replace("use context ", "").Trim(' ');
                    contextType = contextTypes[contextName];
                    continue;
                }

                // action statements
                Match actionMatch = ActionLineRegex().Match(line);
                if (!actionMatch.Success)
                {
                    Console.WriteLine($"failed to recognize line: {line}");
                    continue;
                }

                string actionName = actionMatch.Groups["actionName"].Value.Trim();
                string[] values = actionMatch.Groups["values"].Value.Split('|').Select(x => x.Trim()).ToArray();

                actions.Add(new ScriptAction(actionName, values));
            }

            if (contextType == null)
            {
                Console.WriteLine("context not specified");
                continue;
            }

            // prepare for writeout
            StringBuilder outputFileBuilder = new();

            // process the initial lines of a file
            outputFileBuilder.AppendLine($"using {contextType.Namespace};");
            outputFileBuilder.AppendLine("namespace RopeCSharp.Client.Scripts;");
            outputFileBuilder.AppendLine($"public class {Path.GetFileName(script).Replace(".rope", "")}({contextType.Name} context)");
            outputFileBuilder.AppendLine("{");
            outputFileBuilder.AppendLine("public void Run()");
            outputFileBuilder.AppendLine("{");

            // process each action
            foreach (ScriptAction action in actions)
            {
                StringBuilder builder = new();
                builder.Append($"context.{action.Action}(");
                bool first = true;
                foreach (string value in action.Values)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }
                    first = false;
                    builder.Append($"\"{value}\"");
                }
                builder.Append(");");
                outputFileBuilder.AppendLine(builder.ToString());
            }

            // end the file
            outputFileBuilder.AppendLine("}");
            outputFileBuilder.AppendLine("}");

            Console.WriteLine(outputFileBuilder.ToString());

            // output file
            string outputScript = Path.Combine(ScriptPath, script.Replace(".rope", ".cs"));
            if (File.Exists(outputScript))
            {
                File.Delete(outputScript);
            }
            using StreamWriter outputWriter = new(outputScript);
            await outputWriter.WriteAsync(outputFileBuilder.ToString()).ConfigureAwait(false);
        }
    }
}

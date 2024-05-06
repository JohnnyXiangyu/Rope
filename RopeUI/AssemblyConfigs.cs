using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeUI;
/// <summary>
/// What's the point of AssemblyConfigs?
/// Well, since we are using a lot of reflection magic here and it's incredibly inconvenient to work with reflections, attributes and loaded-from-file assemblies,
/// it's better to just compile this tool with whichever assembly you want to use before running it, in the case it's used for C#.
/// </summary>
internal class AssemblyConfigs
{
    public static Type AssemblyEntry { get; set; } = typeof(FirstPlayable.Abstractions.IContext);
}

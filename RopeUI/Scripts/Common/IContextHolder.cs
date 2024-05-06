using Rope.Abstractions.Reflection;

namespace RopeUI.Scripts.Common;
public interface IContextHolder
{
    ContextType? Context { get; }
}

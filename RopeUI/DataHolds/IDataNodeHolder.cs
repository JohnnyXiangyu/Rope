using Rope.Abstractions.Models;

namespace RopeUI.DataHolds;
internal interface IDataNodeHolder
{
    ScriptNode? DataNode { get; }
}

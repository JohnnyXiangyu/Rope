using Rope.Abstractions.Models;

namespace RopeUI.DataHolds;
internal interface IDataNodeHolder
{
    RopeNode? DataNode { get; }
}

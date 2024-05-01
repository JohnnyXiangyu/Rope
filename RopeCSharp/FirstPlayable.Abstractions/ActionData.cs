using Rope.Abstractions.CSharpAttributes;

namespace FirstPlayable.Abstractions;
[ValueType]
public record ActionData(string Data1, string Data2, long TimeStamp);

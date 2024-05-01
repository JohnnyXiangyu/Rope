namespace RopeCSharp.Serialization;
public sealed class Scope(SerializationContext context, int depth) : IDisposable
{
    public int Depth { get => depth; }

    public void Dispose()
    {
        context.ReleaseScope();
    }
}

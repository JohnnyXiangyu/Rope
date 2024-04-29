namespace RopeCSharp.Serialization;
internal class Scope(SerializationContext context, int depth) : IDisposable
{
    public int Depth { get => depth; }

    public void Dispose()
    {
        context.ReleaseScope();
    }
}

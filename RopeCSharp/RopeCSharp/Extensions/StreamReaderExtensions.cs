namespace RopeCSharp.Extensions;
internal static class StreamReaderExtensions
{
    public static async IAsyncEnumerable<string> ReadAllLinesAsync(this StreamReader reader)
    {
        while (true)
        {
            string? nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
            if (nextLine == null)
                break;

            yield return nextLine;
        }
    }
}

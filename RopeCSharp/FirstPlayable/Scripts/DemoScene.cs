using FirstPlayable.Abstractions;
namespace RopeCSharp.Client.Scripts;
public class DemoScene(IContext context)
{
public void Run()
{
context.Dialogue("Person1", "Hello world");
context.Dialogue("Pseron2", "Owo");
context.Dialogue("Person3", "this is the last line.");
}
}

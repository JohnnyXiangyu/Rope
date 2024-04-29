using FirstPlayable.Abstractions;
namespace FirstPlayable.Scripts;
public class DemoScene(IContext context)
{
    public void MainNode()
    {
        context.Dialogue("Person 1", "Hello");
        context.Dialogue("Person 2", "OwO");
        context.Dialogue("Person 3", "This is the end of the scrip", "YELLS");
        context.GetPoints(3, "completing the first playable");
    }
    public void Run()
    {
        MainNode();
    }
}

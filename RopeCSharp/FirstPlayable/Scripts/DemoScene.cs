using FirstPlayable.Abstractions;
namespace FirstPlayable.Scripts;
public class DemoScene(IContext context)
{
    public enum States
    {
        MainNode,
        SecondNode,
        Terminate
    }
    public States NextState { get; private set; } = States.MainNode;
    public void MainNode()
    {
        context.Dialogue("Person 1", "Hello");
        context.Dialogue("Person 2", "OwO");
        context.Dialogue("Person 3", "This is the end of the scrip", "YELLS");
        context.GetPoints(3, "completing the first playable");
        context.Branch("Next paragraph", "Terminate early");
        switch (context.Choice)
        {
            case 0:
            {
                NextState = States.SecondNode;
                break;
            }
            case 1:
            {
                NextState = States.Terminate;
                break;
            }
            default:
            {
                NextState = States.Terminate;
                break;
            }
        }
    }
    public void SecondNode()
    {
        context.Dialogue("Person 4", "You've reached the end of this story.", "says calmly");
        NextState = States.Terminate;
    }
    public void Run()
    {
        while (true)
        {
            switch (NextState)
            {
                case States.MainNode:
                {
                    MainNode();
                    break;
                }
                case States.SecondNode:
                {
                    SecondNode();
                    break;
                }
                case States.Terminate:
                {
                    return;
                }
            }
        }
    }
}

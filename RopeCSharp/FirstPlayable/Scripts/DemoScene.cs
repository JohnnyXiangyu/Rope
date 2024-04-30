using FirstPlayable.Abstractions;
using System.Collections;
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
    public IEnumerable MainNode()
    {
        context.Dialogue("Person 1", "Hello");
        yield return null;
        context.Dialogue("Person 2", "OwO");
        yield return null;
        context.Dialogue("Person 3", "This is the end of the scrip", "YELLS");
        yield return null;
        context.GetPoints(3, "completing the first playable");
        yield return null;
        context.Branch("Play again", "Next paragraph", "Terminate early");
        yield return null;
        switch (context.Choice)
        {
            case 0:
            {
                NextState = States.MainNode;
                break;
            }
            case 1:
            {
                NextState = States.SecondNode;
                break;
            }
            case 2:
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
    public IEnumerable SecondNode()
    {
        context.Dialogue("Person 4", "You've reached the end of this story.", "says calmly");
        yield return null;
        NextState = States.Terminate;
    }
    public IEnumerable Run()
    {
        while (true)
        {
            if (NextState == States.Terminate)
            {
                break;
            }
            switch (NextState)
            {
                case States.MainNode:
                {
                    foreach (object? _ in MainNode())
                    {
                        yield return null;
                    }
                    break;
                }
                case States.SecondNode:
                {
                    foreach (object? _ in SecondNode())
                    {
                        yield return null;
                    }
                    break;
                }
            }
        }
    }
}

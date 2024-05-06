using FirstPlayable.Abstractions;
using System.Collections;
namespace FirstPlayable.Scripts;
public class RoutineOne(IContext context)
{
    public enum States
    {
        Node_1,
        Node_2,
        Node_3,
        Node_4,
        Terminate
    }
    public States NextState { get; private set; } = States.Node_1;
    public IEnumerable Node_1()
    {
        yield return null;
        switch (context.Choice)
        {
            case 0:
                {
                    NextState = States.Node_2;
                    break;
                }
            case 1:
                {
                    NextState = States.Node_4;
                    break;
                }
            default:
                {
                    NextState = States.Terminate;
                    break;
                }
        }
    }
    public IEnumerable Node_2()
    {
        yield return null;
        NextState = States.Terminate;
    }
    public IEnumerable Node_3()
    {
        yield return null;
        switch (context.Choice)
        {
            case 0:
                {
                    NextState = States.Node_1;
                    break;
                }
            case 1:
                {
                    NextState = States.Node_2;
                    break;
                }
            default:
                {
                    NextState = States.Terminate;
                    break;
                }
        }
    }
    public IEnumerable Node_4()
    {
        yield return null;
        NextState = States.Terminate;
    }
    public IEnumerable Run()
    {
        while (NextState != States.Terminate)
        {
            switch (NextState)
            {
                case States.Node_1:
                    {
                        foreach (object? _ in Node_1())
                        {
                            yield return null;
                        }
                        break;
                    }
                case States.Node_2:
                    {
                        foreach (object? _ in Node_2())
                        {
                            yield return null;
                        }
                        break;
                    }
                case States.Node_3:
                    {
                        foreach (object? _ in Node_3())
                        {
                            yield return null;
                        }
                        break;
                    }
                case States.Node_4:
                    {
                        foreach (object? _ in Node_4())
                        {
                            yield return null;
                        }
                        break;
                    }
            }
        }
    }
}


namespace FirstPlayable.Abstractions;
public interface IContext
{
    void Dialogue(string name, string line, string verb = "says");
}

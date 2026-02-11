namespace Lox.Callables;

public class LoxClass : ILoxCallable
{
    public LoxClass(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public int Arity
        => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
        => new LoxInstance(this);

    public override string ToString()
        => $"<class {Name}>";
}
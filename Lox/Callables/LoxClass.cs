namespace Lox.Callables;

public class LoxClass : ILoxCallable
{
    private readonly Dictionary<string, LoxFunction> _methods;
    
    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        _methods = methods;
    }

    public string Name { get; }

    public int Arity
        => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
        => new LoxInstance(this);

    public LoxFunction? FindMethod(string name)
        => _methods.GetValueOrDefault(name);
    
    public override string ToString()
        => $"<class {Name}>";
}
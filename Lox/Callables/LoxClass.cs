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
        => FindMethod("init")?.Arity ?? 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    public LoxFunction? FindMethod(string name)
        => _methods.GetValueOrDefault(name);
    
    public override string ToString()
        => $"<class {Name}>";
}
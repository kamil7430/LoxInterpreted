namespace Lox.Callables;

public class LoxClass : ILoxCallable
{
    private readonly Dictionary<string, LoxFunction> _methods;
    private readonly Dictionary<string, LoxFunction> _staticMethods;
    
    public LoxClass(string name, Dictionary<string, LoxFunction> methods, Dictionary<string, LoxFunction> staticMethods)
    {
        Name = name;
        _methods = methods;
        _staticMethods = staticMethods;
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
        => _methods.GetValueOrDefault(name) ?? _staticMethods.GetValueOrDefault(name);

    public LoxFunction? FindStaticMethod(Token name)
    {
        var staticMethod = _staticMethods.GetValueOrDefault(name.Lexeme);
        if (staticMethod != null)
            return staticMethod;
        if (FindMethod(name.Lexeme) != null)
            throw new RuntimeErrorException(name, "Can't access non-static methods from static context.");
        return null;
    }

    public override string ToString()
        => $"<class {Name}>";
}
using Lox.Callables;

namespace Lox;

public class LoxInstance
{
    private readonly LoxClass _class;
    private readonly Dictionary<string, object?> _fields = [];

    public LoxInstance(LoxClass @class)
    {
        _class = @class;
    }

    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var value))
            return value;

        var method = _class.FindMethod(name.Lexeme);
        if (method != null)
            return method.Bind(this);
        
        throw new RuntimeErrorException(name, $"Undefined property {name.Lexeme}.");
    }

    public void Set(Token name, object? value)
        => _fields[name.Lexeme] = value;

    public override string ToString()
        => $"<{_class.Name} instance>";
}
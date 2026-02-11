namespace Lox;

public class Environment
{
    public class NotInitialized {}
    
    private readonly Environment? _enclosing;
    private readonly Dictionary<string, object?> _values = [];

    public Environment(Environment? enclosing = null)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, object? value)
        => _values[name] = value;

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            if (value is NotInitialized)
                throw new RuntimeErrorException(name, $"Access to uninitialized variable {name.Lexeme}!");
            return value;
        }

        if (_enclosing != null)
            return _enclosing.Get(name);

        throw new RuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
    }

    public void Assign(Token name, object? value)
    {
        if (!_values.ContainsKey(name.Lexeme))
        {
            if (_enclosing == null) 
                throw new RuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
            _enclosing.Assign(name, value);
            return;
        }

        _values[name.Lexeme] = value;
    }

    public object? GetAt(int distance, string name)
        => Ancestor(distance)._values[name];

    private Environment Ancestor(int distance)
    {
        var environment = this;
        for (int i = 0; i < distance; i++)
            environment = environment._enclosing;
        return environment;
    }

    public void AssignAt(int distance, Token name, object? value)
        => Ancestor(distance)._values[name.Lexeme] = value;
}
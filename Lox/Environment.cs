namespace Lox;

public class Environment
{
    private Dictionary<string, object?> _values = [];

    public void Define(string name, object? value)
        => _values[name] = value;

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
            return value;

        throw new RuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
    }
}
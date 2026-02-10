using Lox.Statements;

namespace Lox.Callables;

public class LoxFunction : ILoxCallable
{
    private readonly Function _declaration;

    public int Arity
        => _declaration.Params.Count;

    public LoxFunction(Function declaration)
    {
        _declaration = declaration;
    }
    
    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(interpreter.Globals);
        for (int i = 0; i < _declaration.Params.Count; i++)
            environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
        interpreter.ExecuteBlock(_declaration.Body, environment);
        return null;
    }

    public override string ToString()
        => $"<fn {_declaration.Name.Lexeme}>";
}
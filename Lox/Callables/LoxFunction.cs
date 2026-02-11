using Lox.Statements;

namespace Lox.Callables;

public class LoxFunction : ILoxCallable
{
    private readonly Function _declaration;
    private readonly Environment _closure;

    public int Arity
        => _declaration.Params.Count;

    public LoxFunction(Function declaration, Environment closure)
    {
        _declaration = declaration;
        _closure = closure;
    }
    
    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(_closure);
        for (int i = 0; i < _declaration.Params.Count; i++)
            environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (ReturnExecutedException e)
        {
            return e.Value;
        }
        return null;
    }

    public override string ToString()
        => $"<fn {_declaration.Name.Lexeme}>";
}
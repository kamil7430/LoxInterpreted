using Lox.Expressions;
using Lox.Statements;

namespace Lox.Callables;

public class LoxFunction : ILoxCallable
{
    private readonly IFunctionlike _declaration;
    private readonly Environment _closure;
    private readonly bool _isInitializer;

    public int Arity
        => _declaration.Params.Count;

    public LoxFunction(IFunctionlike declaration, Environment closure, bool isInitializer)
    {
        _declaration = declaration;
        _closure = closure;
        _isInitializer = isInitializer;
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
            if (_isInitializer)
                return _closure.GetAt(0, "this");
            return e.Value;
        }
        if (_isInitializer)
            return _closure.GetAt(0, "this");
        return null;
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Environment(_closure);
        environment.Define("this", instance);
        return new LoxFunction(_declaration, environment, _isInitializer);
    }

    public override string ToString()
        => _declaration switch
        {
            Function f => $"<fn {f.Name.Lexeme}>",
            Lambda => "<anonymous fn>",
            _ => throw new NotSupportedException(),
        };
}
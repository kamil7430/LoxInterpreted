using Lox.Statements;

namespace Lox.Callables;

public class LoxGetter : LoxFunction
{
    public LoxGetter(IFunctionlike declaration, Environment closure) : base(declaration, closure, false)
    { }

    public override LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Environment(_closure);
        environment.Define("this", instance);
        return new LoxGetter(_declaration, environment);
    }

    public override string ToString()
        => $"<getter {((Getter)_declaration).Name.Lexeme}>";
}
namespace Lox.Callables;

public class Clock : ILoxCallable
{
    public int Arity
        => 0;

    public object? Call(Interpreter interpreter, List<object?> arguments)
        => DateTimeOffset.Now.ToUnixTimeSeconds();

    public override string ToString()
        => "<native fn>";
}
using Lox.Callables;

namespace Lox;

public class LoxInstance
{
    private LoxClass _class;

    public LoxInstance(LoxClass @class)
    {
        _class = @class;
    }

    public override string ToString()
        => $"<{_class.Name} instance>";
}
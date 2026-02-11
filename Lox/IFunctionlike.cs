using Lox.Statements;

namespace Lox;

public interface IFunctionlike
{
    List<Token> Params { get; set; }
    List<Stmt> Body { get; set; }
}
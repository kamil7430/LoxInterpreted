using Lox.Statements;

namespace Lox.Expressions;

public class Lambda : Expr, IFunctionlike
{
    public Lambda(List<Token> @params, List<Stmt> body)
    {
        Params = @params;
        Body = body;
    }

    public List<Token> Params { get; set; }
    public List<Stmt> Body { get; set; } 
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
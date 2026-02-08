using Lox.Expressions;

namespace Lox.Statements;

public class Print : Stmt
{
    public Expr Expression { get; set; }
    
    public Print(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
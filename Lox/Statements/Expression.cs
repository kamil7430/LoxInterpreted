using Lox.Expressions;

namespace Lox.Statements;

public class Expression : Stmt
{
    public Expr Expr { get; set; }
    
    public Expression(Expr expression)
    {
        Expr = expression;
    }

    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
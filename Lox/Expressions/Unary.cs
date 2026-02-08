using Lox.Expressions.Visitors;

namespace Lox.Expressions;

public class Unary : Expr
{
    public Unary(Token @operator, Expr right)
    {
        Operator = @operator;
        Right = right;
    }

    public Token Operator { get; set; }
    public Expr Right { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
using Lox.Expressions.Visitors;

namespace Lox.Expressions;

public class Literal : Expr
{
    public Literal(object? value)
    {
        Value = value;
    }

    public object? Value { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
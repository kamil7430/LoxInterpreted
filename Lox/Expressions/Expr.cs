using Lox.Expressions.Visitors;

namespace Lox.Expressions;

public abstract class Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}
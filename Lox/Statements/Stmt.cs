namespace Lox.Statements;

public abstract class Stmt
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}
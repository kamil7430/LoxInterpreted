namespace Lox.Statements;

public class Break : Stmt
{
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
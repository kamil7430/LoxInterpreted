namespace Lox.Expressions;

public class Assign : Expr
{
    public Assign(Token name, Expr value)
    {
        Name = name;
        Value = value;
    }

    public Token Name { get; set; }
    public Expr Value { get; set; }

    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
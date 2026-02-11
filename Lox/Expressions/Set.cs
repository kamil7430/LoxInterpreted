namespace Lox.Expressions;

public class Set : Expr
{
    public Set(Expr o, Token name, Expr value)
    {
        Object = o;
        Name = name;
        Value = value;
    }

    public Expr Object { get; set; }
    public Token Name { get; set; }
    public Expr Value { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
namespace Lox.Expressions;

public class Get : Expr
{
    public Get(Expr o, Token name)
    {
        Object = o;
        Name = name;
    }

    public Expr Object { get; set; }
    public Token Name { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
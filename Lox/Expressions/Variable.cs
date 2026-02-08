namespace Lox.Expressions;

public class Variable : Expr
{
    public Variable(Token name)
    {
        Name = name;
    }

    public Token Name { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
namespace Lox.Expressions;

public class Logical : Expr
{
    public Logical(Expr left, Token @operator, Expr right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public Expr Left { get; set; }
    public Token Operator { get; set; }
    public Expr Right { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
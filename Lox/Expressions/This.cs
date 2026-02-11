namespace Lox.Expressions;

public class This : Expr
{
    public This(Token keyword)
    {
        Keyword = keyword;
    }

    public Token Keyword { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
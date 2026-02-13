namespace Lox.Expressions;

public class Super : Expr
{
    public Super(Token keyword, Token method)
    {
        Keyword = keyword;
        Method = method;
    }

    public Token Keyword { get; set; }
    public Token Method { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
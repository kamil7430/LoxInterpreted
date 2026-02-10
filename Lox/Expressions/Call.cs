namespace Lox.Expressions;

public class Call : Expr
{
    public Call(Expr callee, Token parenthesis, List<Expr> arguments)
    {
        Callee = callee;
        Parenthesis = parenthesis;
        Arguments = arguments;
    }

    public Expr Callee { get; set; }
    public Token Parenthesis { get; set; }
    public List<Expr> Arguments { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
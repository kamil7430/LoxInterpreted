using Lox.Expressions;

namespace Lox.Statements;

public class Return : Stmt
{
    public Return(Token keyword, Expr? value)
    {
        Keyword = keyword;
        Value = value;
    }

    public Token Keyword { get; set; }
    public Expr? Value { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
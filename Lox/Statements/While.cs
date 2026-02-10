using Lox.Expressions;

namespace Lox.Statements;

public class While : Stmt
{
    public While(Expr condition, Stmt body)
    {
        Condition = condition;
        Body = body;
    }

    public Expr Condition { get; set; }
    public Stmt Body { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
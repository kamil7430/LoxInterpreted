using Lox.Expressions;

namespace Lox.Statements;

public class If : Stmt
{
    public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }

    public Expr Condition { get; set; }
    public Stmt ThenBranch { get; set; }
    public Stmt? ElseBranch { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
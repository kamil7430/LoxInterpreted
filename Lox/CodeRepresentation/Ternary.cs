using Lox.CodeRepresentation.Visitors;

namespace Lox.CodeRepresentation;

public class Ternary : Expr
{
    public Ternary(Expr condition, Expr ifTrue, Expr ifFalse)
    {
        Condition = condition;
        IfTrue = ifTrue;
        IfFalse = ifFalse;
    }

    public Expr Condition { get; set; }
    public Expr IfTrue { get; set; }
    public Expr IfFalse { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
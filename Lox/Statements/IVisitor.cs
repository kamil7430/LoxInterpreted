namespace Lox.Statements;

public interface IVisitor<out T>
{
    T Visit(Expression expression);
    T Visit(Print print);
    T Visit(Var var);
}
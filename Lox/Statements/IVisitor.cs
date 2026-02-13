namespace Lox.Statements;

public interface IVisitor<out T>
{
    T Visit(Expression expression);
    T Visit(Print print);
    T Visit(Var var);
    T Visit(Block block);
    T Visit(If @if);
    T Visit(While @while);
    T Visit(Break @break);
    T Visit(Function function);
    T Visit(Return @return);
    T Visit(Class @class);
    T Visit(Panic panic);
}
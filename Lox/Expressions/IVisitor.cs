namespace Lox.Expressions;

public interface IVisitor<out T>
{
    T Visit(Binary binary);
    T Visit(Grouping grouping);
    T Visit(Literal literal);
    T Visit(Unary unary);
    T Visit(Ternary ternary);
    T Visit(Variable variable);
    T Visit(Assign assign);
    T Visit(Logical logical);
    T Visit(Call call);
    T Visit(Lambda lambda);
    T Visit(Get get);
    T Visit(Set set);
    T Visit(This @this);
    T Visit(Super super);
}
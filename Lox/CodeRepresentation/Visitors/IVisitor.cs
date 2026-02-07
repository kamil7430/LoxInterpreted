namespace Lox.CodeRepresentation.Visitors;

public interface IVisitor<out T>
{
    T Visit(Binary binary);
    T Visit(Grouping grouping);
    T Visit(Literal literal);
    T Visit(Unary unary);
    T Visit(Ternary ternary);
}
using System.Text;

namespace Lox.Expressions.Visitors;

public class AstPrinter : IVisitor<string>
{
    public string Print(Expr expr)
        => expr.Accept(this);
    
    private string Parenthesize(string name, params Expr[] exprs)
    {
        var builder = new StringBuilder();

        builder.Append('(').Append(name);
        foreach (var expr in exprs)
            builder.Append(' ').Append(expr.Accept(this));
        builder.Append(')');

        return builder.ToString();
    }

    public string Visit(Binary binary)
        => Parenthesize(binary.Operator.Lexeme, binary.Left, binary.Right);

    public string Visit(Grouping grouping)
        => Parenthesize("group", grouping.Expression);

    public string Visit(Literal literal)
        => literal.Value?.ToString() ?? "nil";

    public string Visit(Unary unary)
        => Parenthesize(unary.Operator.Lexeme, unary.Right);

    public string Visit(Ternary ternary)
        => Parenthesize("?:", ternary.Condition, ternary.IfTrue, ternary.IfFalse);
}
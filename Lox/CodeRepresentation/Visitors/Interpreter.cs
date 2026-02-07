namespace Lox.CodeRepresentation.Visitors;

public class Interpreter : IVisitor<object?>
{
    public object? Visit(Binary binary)
    {
        var left = Evaluate(binary.Left);
        var right = Evaluate(binary.Right);

        switch (binary.Operator.Type)
        {
            case TokenType.Minus:
                return (double)left - (double)right;
            case TokenType.Plus:
                if (left is double ld && right is double rd)
                    return ld + rd;
                if (left is string ls && right is string rs)
                    return ls + rs;
                break;
            case TokenType.Slash:
                return (double)left / (double)right;
            case TokenType.Star:
                return (double)left * (double)right;
            
            case TokenType.Greater:
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                return (double)left >= (double)right;
            case TokenType.Less:
                return (double)left < (double)right;
            case TokenType.LessEqual:
                return (double)left <= (double)right;
            
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.Equal:
                return IsEqual(left, right);
            
            default:
                throw new NotSupportedException();
        }
    }

    public object? Visit(Grouping grouping)
        => Evaluate(grouping.Expression);

    public object? Visit(Literal literal)
        => literal.Value;

    public object? Visit(Unary unary)
    {
        var right = Evaluate(unary.Right);
        return unary.Operator.Type switch
        {
            TokenType.Minus => -(double)right,
            TokenType.Bang => !IsTruthy(right),
            _ => throw new NotSupportedException(),
        };
    }

    public object? Visit(Ternary ternary)
        => (bool)Evaluate(ternary.Condition) ? Evaluate(ternary.IfTrue) : Evaluate(ternary.IfFalse);

    private object? Evaluate(Expr expr)
        => expr.Accept(this);

    private bool IsTruthy(object? obj)
        => obj switch
        {
            null => false,
            bool b => b,
            _ => true,
        };
    
    private bool IsEqual(object? left, object? right)
        => left is null ? right is null : left.Equals(right);
}
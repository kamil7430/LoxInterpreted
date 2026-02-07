namespace Lox.CodeRepresentation.Visitors;

public class Interpreter : IVisitor<object?>
{
    public void Interpret(Expr expr)
    {
        try
        {
            var result = Evaluate(expr);
            Console.WriteLine(Stringify(result));
        }
        catch (RuntimeErrorException e)
        {
            Program.RuntimeError(e);
        }
    }

    private string Stringify(object? obj)
    {
        switch (obj)
        {
            case null:
                return "nil";
            case double:
                var text = obj.ToString();
                return text.EndsWith(".0") ? text.Substr(0, text.Length - 2) : text;
            default:
                return obj.ToString();
        }
    }

    public object? Visit(Binary binary)
    {
        var left = Evaluate(binary.Left);
        var right = Evaluate(binary.Right);

        switch (binary.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! - (double)right!;
            case TokenType.Plus:
                if (left is double ld)
                {
                    if (right is double rd)
                        return ld + rd;
                    if (right is string rs)
                        return Stringify(ld) + rs;
                }
                if (left is string ls)
                {
                    if (right is double rd)
                        return ls + Stringify(rd);
                    if (right is string rs)
                        return ls + rs;
                }
                throw new RuntimeErrorException(binary.Operator, "Operands must be two numbers, two strings or combination of those!");
            case TokenType.Slash:
                CheckNumberOperands(binary.Operator, left, right);
                if ((double)right! == 0)
                    throw new RuntimeErrorException(binary.Operator, "Cannot divide by zero!");
                return (double)left! / (double)right!;
            case TokenType.Star:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! * (double)right!;
            
            case TokenType.Greater:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! > (double)right!;
            case TokenType.GreaterEqual:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! >= (double)right!;
            case TokenType.Less:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! < (double)right!;
            case TokenType.LessEqual:
                CheckNumberOperands(binary.Operator, left, right);
                return (double)left! <= (double)right!;
            
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
        }
        
        throw new NotSupportedException();
    }

    public object? Visit(Grouping grouping)
        => Evaluate(grouping.Expression);

    public object? Visit(Literal literal)
        => literal.Value;

    public object? Visit(Unary unary)
    {
        var right = Evaluate(unary.Right);
        switch (unary.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(unary.Operator, right);
                return -(double)right!;
            case TokenType.Bang:
                return !IsTruthy(right);
            default:
                throw new NotSupportedException();
        }
    }

    public object? Visit(Ternary ternary)
    {
        var condition = IsTruthy(Evaluate(ternary.Condition));
        return condition ? Evaluate(ternary.IfTrue) : Evaluate(ternary.IfFalse);
    }

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

    private void CheckNumberOperand(Token @operator, object? operand)
    {
        if (operand is not double) 
            throw new RuntimeErrorException(@operator, "Operand must be a number!");
    }

    private void CheckNumberOperands(Token @operator, object? left, object? right)
    {
        if (left is not double)
            throw new RuntimeErrorException(@operator, "Left operand must be a number!");
        if (right is not double)
            throw new RuntimeErrorException(@operator, "Right operand must be a number!");
    }
}
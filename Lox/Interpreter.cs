using Lox.Callables;
using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public class Interpreter : Expressions.IVisitor<object?>, Statements.IVisitor<None?>
{
    private class BreakExecutedException : Exception {}

    public Environment Globals { get; }
    private Environment _environment;

    public bool ShouldPrintEvaluatedExpressions { get; set; } = false;

    public Interpreter()
    {
        Globals = new Environment();
        _environment = Globals;
        Globals.Define("clock", new Clock());
    }
    
    public void Interpret(IEnumerable<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
                Execute(stmt);
        }
        catch (RuntimeErrorException e)
        {
            Program.RuntimeError(e);
        }
    }

    private void Execute(Stmt stmt)
        => stmt.Accept(this);

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
            
            case TokenType.Comma:
                return right;
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

    public object? Visit(Variable variable)
        => _environment.Get(variable.Name);

    public object? Visit(Assign assign)
    {
        var value = Evaluate(assign.Value);
        _environment.Assign(assign.Name, value);
        return value;
    }

    public object? Visit(Logical logical)
    {
        var left = Evaluate(logical.Left);

        switch (logical.Operator.Type)
        {
            case TokenType.Or:
                if (IsTruthy(left))
                    return left;
                break;
            case TokenType.And:
                if (!IsTruthy(left))
                    return left;
                break;
        }

        return Evaluate(logical.Right);
    }

    public object? Visit(Call call)
    {
        var function = Evaluate(call.Callee) is ILoxCallable callable ? 
            callable : throw new RuntimeErrorException(call.Parenthesis, "Can only call functions and classes.");
        var arguments = call.Arguments.Select(Evaluate).ToList();
        if (arguments.Count != function.Arity)
            throw new RuntimeErrorException(call.Parenthesis, $"Expected {function.Arity} arguments, got {arguments.Count}.");
        return function.Call(this, arguments);
    }

    public None? Visit(Expression expression)
    {
        var value = Evaluate(expression.Expr);
        if (ShouldPrintEvaluatedExpressions)
            Console.WriteLine(Stringify(value));
        return null;
    }

    public None? Visit(Print print)
    {
        var value = Evaluate(print.Expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public None? Visit(Var var)
    {
        object? value = new Environment.NotInitialized();
        if (var.Initializer != null)
            value = Evaluate(var.Initializer);
        
        _environment.Define(var.Name.Lexeme, value);
        return null;
    }

    public None? Visit(Block block)
    {
        ExecuteBlock(block.Statements, new Environment(_environment));
        return null;
    }

    public None? Visit(If @if)
    {
        if (IsTruthy(Evaluate(@if.Condition)))
            Execute(@if.ThenBranch);
        else if (@if.ElseBranch != null)
            Execute(@if.ElseBranch);
        return null;
    }

    public None? Visit(While @while)
    {
        try
        {
            while (IsTruthy(Evaluate(@while.Condition)))
                Execute(@while.Body);
        }
        catch (BreakExecutedException) { }
        return null;
    }

    public None? Visit(Break @break)
        => throw new BreakExecutedException();

    public None? Visit(Function function)
    {
        var fun = new LoxFunction(function);
        _environment.Define(function.Name.Lexeme, fun);
        return null;
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        var previous = _environment;
        try
        {
            _environment = environment;
            foreach (var stmt in statements)
                Execute(stmt);
        }
        finally
        {
            _environment = previous;
        }
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
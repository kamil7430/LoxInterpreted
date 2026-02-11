using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public class Resolver : Expressions.IVisitor<None?>, Statements.IVisitor<None?>
{
    private enum FunctionType
    {
        None,
        Function,
        Lambda,
        Method,
    }

    private enum ClassType
    {
        None,
        Class,
    }

    private class VariableDetails(Token name, bool initialized, bool used)
    {
        public Token Name { get; set; } = name;
        public bool Initialized { get; set; } = initialized;
        public bool Used { get; set; } = used;
    }

    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, VariableDetails>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;

    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
        => statements.ForEach(Resolve);

    private void Resolve(Stmt statement)
        => statement.Accept(this);

    private void Resolve(Expr expression)
        => expression.Accept(this);

    public None? Visit(Binary binary)
    {
        Resolve(binary.Left);
        Resolve(binary.Right);
        return null;
    }

    public None? Visit(Grouping grouping)
    {
        Resolve(grouping.Expression);
        return null;
    }

    public None? Visit(Literal literal)
        => null;

    public None? Visit(Unary unary)
    {
        Resolve(unary.Right);
        return null;
    }

    public None? Visit(Ternary ternary)
    {
        Resolve(ternary.Condition);
        Resolve(ternary.IfTrue);
        Resolve(ternary.IfFalse);
        return null;
    }

    public None? Visit(Variable variable)
    {
        if (_scopes.Count > 0 && _scopes.Peek().TryGetValue(variable.Name.Lexeme, out var details))
            if (!details.Initialized)
                Program.Error(variable.Name, "Can't read local variable in its own initializer.");
        
        ResolveLocal(variable, variable.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        Stack<Dictionary<string, VariableDetails>> tmp = new();
        int i = -1;
        while (_scopes.Count > 0)
        {
            i++;
            var current = _scopes.Pop();
            tmp.Push(current);
            if (current.TryGetValue(name.Lexeme, out var details))
            {
                _interpreter.Resolve(expr, i);
                details.Used = true;
                break;
            }
        }
        while (tmp.Count > 0)
            _scopes.Push(tmp.Pop());
    }

    public None? Visit(Assign assign)
    {
        Resolve(assign.Value);
        ResolveLocal(assign, assign.Name);
        return null;
    }

    public None? Visit(Logical logical)
    {
        Resolve(logical.Left);
        Resolve(logical.Right);
        return null;
    }

    public None? Visit(Call call)
    {
        Resolve(call.Callee);
        call.Arguments.ForEach(Resolve);
        return null;
    }

    public None? Visit(Lambda lambda)
    {
        ResolveFunctionlike(lambda, FunctionType.Lambda);
        return null;
    }

    public None? Visit(Get get)
    {
        Resolve(get.Object);
        return null;
    }

    public None? Visit(Set set)
    {
        Resolve(set.Object);
        Resolve(set.Value);
        return null;
    }

    public None? Visit(This @this)
    {
        if (_currentClass == ClassType.None)
            Program.Error(@this.Keyword, "Can't use 'this' outside of a class.");
        else
            ResolveLocal(@this, @this.Keyword);
        return null;
    }

    public None? Visit(Expression expression)
    {
        Resolve(expression.Expr);
        return null;
    }

    public None? Visit(Print print)
    {
        Resolve(print.Expression);
        return null;
    }

    public None? Visit(Var var)
    {
        Declare(var.Name);
        if (var.Initializer != null)
            Resolve(var.Initializer);
        Define(var.Name);
        return null;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count <= 0)
            return;
        if (!_scopes.Peek().TryAdd(name.Lexeme, new VariableDetails(name, false, false)))
            Program.Error(name, "A variable with this name is already in this scope.");
    }

    private void Define(Token name)
    {
        if (_scopes.Count <= 0)
            return;
        _scopes.Peek()[name.Lexeme].Initialized = true;
    }

    public None? Visit(Block block)
    {
        BeginScope();
        Resolve(block.Statements);
        EndScope();
        return null;
    }

    private void BeginScope()
        => _scopes.Push(new Dictionary<string, VariableDetails>());

    private void EndScope()
    {
        var scope = _scopes.Pop();
        foreach (var detail in scope.Values)
            if (!detail.Used)
                Program.Error(detail.Name, "Unused variable.");
    }

    public None? Visit(If @if)
    {
        Resolve(@if.Condition);
        Resolve(@if.ThenBranch);
        if (@if.ElseBranch != null)
            Resolve(@if.ElseBranch);
        return null;
    }

    public None? Visit(While @while)
    {
        Resolve(@while.Condition);
        Resolve(@while.Body);
        return null;
    }

    public None? Visit(Break @break)
        => null;

    public None? Visit(Function function)
    {
        Declare(function.Name);
        Define(function.Name);
        
        ResolveFunctionlike(function, FunctionType.Function);
        return null;
    }

    private void ResolveFunctionlike(IFunctionlike functionlike, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;
        
        BeginScope();
        foreach (var param in functionlike.Params)
        {
            Declare(param);
            Define(param);
        }
        Resolve(functionlike.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    public None? Visit(Return @return)
    {
        if (_currentFunction == FunctionType.None)
            Program.Error(@return.Keyword, "Can't return from top-level code.");
        
        if (@return.Value != null)
            Resolve(@return.Value);
        return null;
    }

    public None? Visit(Class @class)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.Class;
        
        Declare(@class.Name);
        Define(@class.Name);

        BeginScope();
        _scopes.Peek()["this"] = new VariableDetails(null, true, true);
        
        foreach (var method in @class.Methods)
        {
            var declaration = FunctionType.Method;
            ResolveFunctionlike(method, declaration);
        }
        
        EndScope();

        _currentClass = enclosingClass;
        return null;
    }
}
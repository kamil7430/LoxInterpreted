using System.Linq.Expressions;
using Lox.Expressions;
using Lox.Statements;
using Expression = Lox.Statements.Expression;

namespace Lox;

/*
program        → declaration* EOF ;
declaration    → classDecl | funDecl | varDecl | statement ;
statement      → exprStmt | forStmt | ifStmt | printStmt | returnStmt
               | whileStmt | breakStmt | block ;

classDecl      → "class" IDENTIFIER "{" ( "static"? ( function | getter ) )* "}" ;
funDecl        → "fun" function ;
function       → IDENTIFIER "(" parameters? ")" block ;
getter         → IDENTIFIER block ;
varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
exprStmt       → expression ";" ;
forStmt        → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" 
               expression? ")" statement ;
ifStmt         → "if" "(" expression ")" statement ( "else" statement )? ;
printStmt      → "print" expression ";" ;
returnStmt     → "return" expression? ";" ;
whileStmt      → "while" "(" expression ")" statement ;
breakStmt      → "break" ";" ;
block          → "{" declaration* "}" ;
 
expression     → comma ;
comma          → assignment ( "," assignment )* ;
assignment     → ( call "." )? IDENTIFIER "=" assignment | conditional ;
conditional    → logic_or ( "?" logic_or ":" logic_or )* ;
logic_or       → logic_and ( "or" logic_and )* ;
logic_and      → equality ( "and" equality )* ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           → factor ( ( "-" | "+" ) factor )* ;
factor         → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary | call ;
call           → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
primary        → NUMBER | STRING | "this" | "true" | "false" | "nil"
               | "(" expression ")" | IDENTIFIER | lambda ;
lambda         → "fun" "(" parameters? ")" block ;

parameters     → IDENTIFIER ( "," IDENTIFIER )* ;
arguments      → assignment ( "," assignment )* ;
*/

public class Parser
{
    private class ParseErrorException : Exception {}
    
    private readonly List<Token> _tokens;
    private int _current = 0;
    private bool _isInsideLoop = false;
    
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    // program → declaration* EOF ;
    public List<Stmt> Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
            statements.Add(Declaration());
        return statements;
    }

    // declaration → classDecl | funDecl | varDecl | statement ;
    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.Class))
                return ClassDeclaration();
            if (Match(TokenType.Fun))
                return Function("function");
            if (Match(TokenType.Var))
                return VarDeclaration();

            return Statement();
        }
        catch (ParseErrorException e)
        {
            Synchronize();
            return null;
        }
    }

    // classDecl → "class" IDENTIFIER "{" ( "static"? ( function | getter ) )* "}" ;
    private Stmt ClassDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected class name.");
        Consume(TokenType.LeftBrace, "Expected '{' before class body.");

        List<Function> methods = [];
        List<Function> staticMethods = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (Match(TokenType.Static))
                staticMethods.Add(Function("static method", true));
            else
                methods.Add(Function("method", true));
        }

        Consume(TokenType.RightBrace, "Expected '}' after class body.");
        return new Class(name, methods, staticMethods);
    }

    // funDecl    → "fun" function ;
    // function   → IDENTIFIER "(" parameters? ")" block ;
    // getter     → IDENTIFIER block ;
    // parameters → IDENTIFIER ( "," IDENTIFIER )* ;
    private Function Function(string kind, bool allowGetter = false)
    {
        var name = Consume(TokenType.Identifier, $"Expected {kind} name.");

        if (allowGetter && Match(TokenType.LeftBrace))
            return new Getter(name, Block());
        
        Consume(TokenType.LeftParen, $"Expected '(' after {kind} name.");
        
        List<Token> parameters = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                    Error(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter name."));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expected ')' after parameter list.");

        Consume(TokenType.LeftBrace, $"Expected '{{' before {kind} body.");
        var body = Block();
        return new Function(name, parameters, body);
    }

    // varDecl → "var" IDENTIFIER ( "=" expression )? ";" ;
    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expected variable name.");

        Expr? initializer = null;
        if (Match(TokenType.Equal))
            initializer = Expression();

        Consume(TokenType.Semicolon, "Expected ';' after variable declaration.");
        return new Var(name, initializer);
    }

    // statement → exprStmt | forStmt | ifStmt | printStmt | returnStmt | whileStmt | breakStmt | block ;
    private Stmt Statement()
    {
        if (Match(TokenType.For))
            return ForStatement();
        if (Match(TokenType.If))
            return IfStatement();
        if (Match(TokenType.Print))
            return PrintStatement();
        if (Match(TokenType.Return))
            return ReturnStatement();
        if (Match(TokenType.While))
            return WhileStatement();
        if (Match(TokenType.Break))
            return BreakStatement();
        if (Match(TokenType.LeftBrace))
            return new Block(Block());
        return ExpressionStatement();
    }

    // forStmt → "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement ;
    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after 'for'.");

        Stmt? initializer;
        if (Match(TokenType.Semicolon))
            initializer = null;
        else if (Match(TokenType.Var))
            initializer = VarDeclaration();
        else
            initializer = ExpressionStatement();

        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
            condition = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after loop condition.");

        Expr? increment = null;
        if (!Check(TokenType.RightParen))
            increment = Expression();
        Consume(TokenType.RightParen, "Expected ')' after for clauses.");

        _isInsideLoop = true;
        var body = Statement();
        _isInsideLoop = false;

        if (increment != null)
            body = new Block([
                body,
                new Expression(increment),
            ]);

        condition ??= new Literal(true);
        body = new While(condition, body);

        if (initializer != null)
            body = new Block([
                initializer,
                body
            ]);

        return body;
    }

    // exprStmt → expression ";" ;
    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after expression!");
        return new Expression(expr);
    }

    // ifStmt → "if" "(" expression ")" statement ( "else" statement )? ;
    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expected ')' after if condition.");

        var thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
            elseBranch = Statement();

        return new If(condition, thenBranch, elseBranch);
    }
    
    // printStmt → "print" expression ";" ;
    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after value!");
        return new Print(value);
    }

    // returnStmt → "return" expression? ";" ;
    private Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.Semicolon))
            value = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after return value.");
        return new Return(keyword, value);
    }

    // whileStmt → "while" "(" expression ")" statement ;
    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expected '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expected ')' after while condition.");
        _isInsideLoop = true;
        var body = Statement();
        _isInsideLoop = false;
        return new While(condition, body);
    }

    // breakStmt → "break" ";" ;
    private Stmt BreakStatement()
    {
        if (!_isInsideLoop)
            throw Error(Previous(), "'break' token outside of a loop!");
        Consume(TokenType.Semicolon, "Expected ';' after 'break'.");
        return new Break();
    }
    
    // block → "{" declaration* "}" ;
    private List<Stmt> Block()
    {
        List<Stmt> statements = [];
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
            statements.Add(Declaration());
        Consume(TokenType.RightBrace, "Expected '}' after block.");
        return statements;
    }

    // expression → comma ;
    private Expr Expression()
        => Comma();
    
    // comma → assignment ( "," assignment )* ;
    private Expr Comma()
    {
        var expr = Assignment();
        while (Match(TokenType.Comma))
            expr = new Binary(expr, Previous(), Assignment());
        return expr;
    }
    
    // assignment → ( call "." )? IDENTIFIER "=" assignment | conditional ;
    private Expr Assignment()
    {
        var expr = Conditional();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();
            if (expr is Variable varExpr)
                return new Assign(varExpr.Name, value);
            if (expr is Get getExpr)
                return new Set(getExpr.Object, getExpr.Name, value);

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    // conditional → logic_or ( "?" logic_or ":" logic_or )* ;
    private Expr Conditional()
    {
        Stack<Expr> exprs = [];
        exprs.Push(LogicOr());
        
        while (Match(TokenType.QuestionMark))
        {
            exprs.Push(LogicOr());
            if (Match(TokenType.Colon))
                exprs.Push(LogicOr());
            else
                throw Error(Peek(), "Expected ':' in conditional expression.");
        }
        
        var expr = exprs.Pop();
        while (exprs.Count > 0)
        {
            var ifTrue = exprs.Pop();
            var condition = exprs.Pop();
            expr = new Ternary(condition, ifTrue, expr);
        }

        return expr;
    }

    // logic_or → logic_and ( "or" logic_and )* ;
    private Expr LogicOr()
    {
        var expr = LogicAnd();
        while (Match(TokenType.Or))
            expr = new Logical(expr, Previous(), LogicAnd());
        return expr;
    }

    // logic_and → equality ( "and" equality )* ;
    private Expr LogicAnd()
    {
        var expr = Equality();
        while (Match(TokenType.And))
            expr = new Logical(expr, Previous(), Equality());
        return expr;
    }

    // equality → comparison ( ( "!=" | "==" ) comparison )* ;
    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
            expr = new Binary(expr, Previous(), Comparison());
        return expr;
    }

    // comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr Comparison()
    {
        var expr = Term();
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            expr = new Binary(expr, Previous(), Term());
        return expr;
    }

    // term → factor ( ( "-" | "+" ) factor )* ;
    private Expr Term()
    {
        var expr = Factor();
        while (Match(TokenType.Minus, TokenType.Plus))
            expr = new Binary(expr, Previous(), Factor());
        return expr;
    }

    // factor → unary ( ( "/" | "*" ) unary )* ;
    private Expr Factor()
    {
        var expr = Unary();
        while (Match(TokenType.Slash, TokenType.Star))
            expr = new Binary(expr, Previous(), Unary());
        return expr;
    }

    // unary → ( "!" | "-" ) unary | call ;
    private Expr Unary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus))
            return Call();
        return new Unary(Previous(), Unary());
    }

    // call → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
    private Expr Call()
    {
        var expr = Primary();
        while (true)
        {
            if (Match(TokenType.LeftParen))
                expr = FinishCall(expr);
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expected property name after '.'.");
                expr = new Get(expr, name);
            }
            else 
                break;
        }
        return expr;
    }

    // primary → NUMBER | STRING | "this" | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER | lambda ;
    private Expr Primary()
    {
        if (Match(TokenType.True))
            return new Literal(true);
        if (Match(TokenType.False))
            return new Literal(false);
        if (Match(TokenType.Nil))
            return new Literal(null);
        if (Match(TokenType.Number, TokenType.String))
            return new Literal(Previous().Literal);
        if (Match(TokenType.This))
            return new This(Previous());
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expected ')' after expression!");
            return new Grouping(expr);
        }
        if (Match(TokenType.Identifier))
            return new Variable(Previous());
        if (Match(TokenType.Fun))
            return Lambda();

        throw Error(Peek(), "Expected expression.");
    }

    // lambda → "fun" "(" parameters? ")" block ;
    private Expr Lambda()
    {
        Consume(TokenType.LeftParen, "Expected '(' after 'fun' in lambda declaration.");
        
        List<Token> parameters = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                    Error(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(TokenType.Identifier, "Expected parameter identifier."));
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightParen, "Expected ')' after parameter list.");
        Consume(TokenType.LeftBrace, "Expected '{' after parameters.");
        var body = Block();
        return new Lambda(parameters, body);
    }

    // arguments → assignment ( "," assignment )* ;
    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                    Error(Peek(), "Can't have more than 255 arguments.");
                arguments.Add(Assignment());
            } while (Match(TokenType.Comma));
        }
        var paren = Consume(TokenType.RightParen, "Expected ')' after arguments.");
        return new Call(callee, paren, arguments);
    }

    private Token Consume(TokenType type, string message)
        => Check(type) ? Advance() : throw Error(Peek(), message);

    private ParseErrorException Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseErrorException();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) 
                return;

            switch (Peek().Type)
            {
                case TokenType.Class: 
                case TokenType.Fun: 
                case TokenType.Var: 
                case TokenType.For: 
                case TokenType.If: 
                case TokenType.While: 
                case TokenType.Print: 
                case TokenType.Return:
                    return;
            }
            
            Advance();
        }
    }
    
    private Token Advance()
    {
        if (!IsAtEnd())
            _current++;
        return Previous();
    }
    
    private Token Previous()
        => _tokens[_current - 1];
    
    private Token Peek()
        => _tokens[_current];

    private bool IsAtEnd()
        => Peek().Type == TokenType.Eof;

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
            return false;
        return Peek().Type == type;
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check)) 
            return false;
        Advance();
        return true;
    }
}
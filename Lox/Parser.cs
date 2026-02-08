using Lox.Expressions;
using Lox.Statements;

namespace Lox;

/*
program        → declaration* EOF ;
declaration    → varDecl | statement ;
statement      → exprStmt | printStmt ;

varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
exprStmt       → expression ";" ;
printStmt      → "print" expression ";" ;
 
expression     → assignment ;
assignment     → IDENTIFIER "=" assignment | conditional ;
conditional    → comma ( "?" comma ":" comma )* ;
comma          → equality ( "," equality )* ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           → factor ( ( "-" | "+" ) factor )* ;
factor         → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | primary ;
primary        → NUMBER | STRING | "true" | "false" | "nil"
               | "(" expression ")" | IDENTIFIER ;
*/

public class Parser
{
    private class ParseErrorException : Exception {}
    
    private readonly List<Token> _tokens;
    private int _current = 0;
    
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

    // declaration → varDecl | statement ;
    private Stmt Declaration()
    {
        try
        {
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

    // statement → exprStmt | printStmt ;
    private Stmt Statement()
    {
        if (Match(TokenType.Print))
            return PrintStatement();
        return ExpressionStatement();
    }

    // exprStmt → expression ";" ;
    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after expression!");
        return new Expression(expr);
    }

    // printStmt → "print" expression ";" ;
    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expected ';' after value!");
        return new Print(value);
    }

    // expression → assignment ;
    private Expr Expression()
        => Assignment();
    
    // assignment → IDENTIFIER "=" assignment | conditional ;
    private Expr Assignment()
    {
        var expr = Conditional();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();
            if (expr is Variable varExpr)
                return new Assign(varExpr.Name, value);

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    // conditional → comma ( "?" comma ":" comma )* ;
    private Expr Conditional()
    {
        Stack<Expr> exprs = [];
        exprs.Push(Comma());
        
        while (Match(TokenType.QuestionMark))
        {
            exprs.Push(Comma());
            if (Match(TokenType.Colon))
                exprs.Push(Comma());
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

    // comma → equality ( "," equality )* ;
    private Expr Comma()
    {
        var expr = Equality();
        while (Match(TokenType.Comma))
            expr = new Binary(expr, Previous(), Equality());
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

    // unary → ( "!" | "-" ) unary | primary ;
    private Expr Unary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus))
            return Primary();
        return new Unary(Previous(), Unary());
    }
    
    // primary → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER ;
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
        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expected ')' after expression!");
            return new Grouping(expr);
        }
        if (Match(TokenType.Identifier))
            return new Variable(Previous());

        throw Error(Peek(), "Expected expression.");
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
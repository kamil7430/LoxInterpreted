using System.Data.Common;

namespace Lox;

public class Scanner
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "and", TokenType.And },
        { "class", TokenType.Class },
        { "else", TokenType.Else },
        { "false", TokenType.False },
        { "for", TokenType.For },
        { "fun", TokenType.Fun },
        { "if", TokenType.If },
        { "nil", TokenType.Nil },
        { "or", TokenType.Or },
        { "print", TokenType.Print },
        { "return", TokenType.Return },
        { "super", TokenType.Super },
        { "this", TokenType.This },
        { "true", TokenType.True },
        { "var", TokenType.Var },
        { "while", TokenType.While }
    };
    
    private readonly string _source;
    private readonly List<Token> _tokens = [];
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    public Scanner(string source)
    {
        _source = source;
    }

    private bool IsAtEnd()
        => _current >= _source.Length;
    
    public IEnumerable<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        
        _tokens.Add(new Token(TokenType.Eof, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else if (Match('*'))
                    MultilineComment();
                else
                    AddToken(TokenType.Slash);
                break;
            
            case ' ': case '\r': case '\t':
                break;
            case '\n':
                _line++;
                break;
            
            case '"':
                String();
                break;
            
            default:
                if (char.IsDigit(c))
                    Number();
                else if (IsAlpha(c))
                    Identifier();
                else
                    Program.Error(_line, "Unexpected character (" + c + ").");
                break;
        }
    }

    private char Advance()
        => _source[_current++];

    private char Peek()
        => IsAtEnd() ? '\0' : _source[_current];

    private char PeekNext()
        => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

    private bool IsAlpha(char c)
        => char.IsLetter(c) || c == '_';

    private bool IsAlphaNumeric(char c)
        => IsAlpha(c) || char.IsDigit(c);
    
    private void AddToken(TokenType type)
        => AddToken(type, null);

    private void AddToken(TokenType type, object? literal)
    {
        var lexeme = _source.Substr(_start, _current);
        _tokens.Add(new Token(type, lexeme, literal, _line));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
            return false;
        if (_source[_current] != expected)
            return false;

        _current++;
        return true;
    }

    private void MultilineComment()
    {
        int nestingLevel = 1;
        
        while (nestingLevel > 0)
        {
            switch (Advance())
            {
                case '\n':
                    _line++;
                    break;
                case '/':
                    if (Match('*'))
                        nestingLevel++;
                    break;
                case '*':
                    if (Match('/'))
                        nestingLevel--;
                    break;
            }
        }
    }
    
    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n')
                _line++;
            Advance();
        }
        
        if (IsAtEnd())
            Program.Error(_line, "Unterminated string.");

        Advance();
        AddToken(TokenType.String, _source.Substr(_start + 1, _current - 1));
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
            Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();
            while (char.IsDigit(Peek()))
                Advance();
        }
        
        AddToken(TokenType.Number, double.Parse(_source.Substr(_start, _current)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();

        var text = _source.Substr(_start, _current);
        var found = Keywords.TryGetValue(text, out var type);
        if (!found)
            type = TokenType.Identifier;
        
        AddToken(type);
    }
}
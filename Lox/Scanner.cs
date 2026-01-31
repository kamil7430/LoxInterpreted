using System.Data.Common;

namespace Lox;

public class Scanner
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE }
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
        
        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case '-':
                AddToken(TokenType.MINUS);
                break;
            case '+':
                AddToken(TokenType.PLUS);
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case '*':
                AddToken(TokenType.STAR);
                break;
            
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
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
                    AddToken(TokenType.SLASH);
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
        AddToken(TokenType.STRING, _source.Substr(_start + 1, _current - 1));
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
        
        AddToken(TokenType.NUMBER, double.Parse(_source.Substr(_start, _current)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();

        var text = _source.Substr(_start, _current);
        var found = Keywords.TryGetValue(text, out var type);
        if (!found)
            type = TokenType.IDENTIFIER;
        
        AddToken(type);
    }
}
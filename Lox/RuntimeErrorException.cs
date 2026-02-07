namespace Lox;

public class RuntimeErrorException : Exception
{
    public Token Token { get; }

    public RuntimeErrorException(Token token, string? message) : base(message)
    {
        Token = token;
    }
}
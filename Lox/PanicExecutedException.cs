namespace Lox;

public class PanicExecutedException : Exception
{
    public PanicExecutedException(Token keyword, object? value)
    {
        Keyword = keyword;
        Value = value;
    }

    public Token Keyword { get; set; }
    public object? Value { get; set; }
}
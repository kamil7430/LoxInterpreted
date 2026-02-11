namespace Lox;


public class ReturnExecutedException : Exception
{
    public ReturnExecutedException(object? value)
    {
        Value = value;
    }

    public object? Value { get; }
}
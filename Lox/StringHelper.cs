namespace Lox;

public static class StringHelper
{
    public static string Substr(this string str, int start, int stop)
        => str.Substring(start, stop - start);
}
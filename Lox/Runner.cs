namespace Lox;

public static class Runner
{
    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        
        foreach (var token in tokens)
            Console.WriteLine(token);
    }
}
using Lox.CodeRepresentation.Visitors;

namespace Lox;

public static class Runner
{
    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens.ToList());
        var expression = parser.Parse();

        Console.WriteLine(new AstPrinter().Print(expression));
    }
}
using System.Globalization;

namespace Lox;

public class Program
{
    private static bool _hadError = false;
    private static bool _hadRuntimeError = false;
    private static readonly Interpreter _interpreter = new();
    
    private static async Task Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        
        if (args.Length > 1)
        {
            Console.Error.WriteLine("Too many arguments!\nUsage: Lox [script-file]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
            await RunFile(args[0]);
        else
            RunPrompt();
    }

    private static async Task RunFile(string path)
    {
        using var reader = new StreamReader(path);
        var script = await reader.ReadToEndAsync();
        Run(script);
        if (_hadError)
            Environment.Exit(65);
        if (_hadRuntimeError)
            Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
            Run(line);
            _hadError = false;  
        }
    }
    
    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens.ToList());
        var statements = parser.Parse();

        if (_hadError)
            return;

        _interpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
            Report(token.Line, " at end", message);
        else 
            Report(token.Line, " at '" + token.Lexeme + "'", message);
    }

    public static void RuntimeError(RuntimeErrorException e)
    {
        Console.WriteLine(e.Message + "\n[line " + e.Token.Line + ']');
        _hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine("[Line " + line + "] Error" + where + ": " + message);
        _hadError = true;
    }
}
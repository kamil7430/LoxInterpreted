using System.Globalization;

namespace Lox;

public class Program
{
    private static bool _hadError = false;
    
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
        Runner.Run(script);
        if (_hadError)
            Environment.Exit(65);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
            Runner.Run(line);
            _hadError = false;  
        }
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

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine("[Line " + line + "] Error" + where + ": " + message);
        _hadError = true;
    }
}
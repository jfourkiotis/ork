using ork;
using ork.lexer;
using ork.parser;

internal static class OrkDriver
{
    private static void Main()
    {
        Console.WriteLine($"Hello {Environment.UserName}! This is the ORK programming language!");
        Console.WriteLine("Feel free to type in commands");

        var cliArgs = Environment.GetCommandLineArgs();
        if (cliArgs.Length > 1) // assume the second argument is the script to execute
        {
            var script = File.ReadAllText(cliArgs[1]);
            RunFile(script);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunPrompt()
    {
        TreeWalkingInterpreter tw = new();
        do
        {
            Console.Write(">> ");

            var line = Console.ReadLine();
            if (line is null)
            {
                break;
            }

            Run(line, tw);
        } while (true);
    }

    private static void RunFile(string contents)
    {
        TreeWalkingInterpreter tw = new();
        Run(contents, tw);
    }

    private static void Run(string line, TreeWalkingInterpreter tw)
    {
        var lexer = new Lexer(line);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        if (parser.Errors.Count != 0)
        {
            foreach (var msg in parser.Errors)
                Console.WriteLine(msg);
            return;
        }

	    var t0 = System.Diagnostics.Stopwatch.StartNew();
        var result = tw.Eval(program);
	    t0.Stop();
        if (result is null) return;
        Console.WriteLine(result);
        Console.WriteLine($"[elapsed time: {t0.Elapsed.TotalMilliseconds}ms]");
    }
}

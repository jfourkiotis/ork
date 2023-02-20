using ork;
using ork.lexer;
using ork.parser;
using Environment = ork.Environment;

internal static class OrkDriver
{
    private static void Main()
    {
        Console.WriteLine($"Hello {System.Environment.UserName}! This is the ORK programming language!");
        Console.WriteLine("Feel free to type in commands");

        var cliArgs = System.Environment.GetCommandLineArgs();
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
        Environment env = new Environment();
        do
        {
            Console.Write(">> ");

            var line = Console.ReadLine();
            if (line is null)
            {
                break;
            }

            Run(line, tw, env);
        } while (true);
    }

    private static void RunFile(string contents)
    {
        TreeWalkingInterpreter tw = new();
        Environment env = new Environment();
        Run(contents, tw, env);
    }

    private static void Run(string line, TreeWalkingInterpreter tw, Environment env)
    {
        try
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
            var result = tw.Eval(program, env);
            t0.Stop();
            Console.WriteLine(ork.Object.ToString(result));
            Console.WriteLine($"[elapsed time: {t0.Elapsed.TotalMilliseconds}ms]");
        }
        catch (OrkRuntimeException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
using ork.lexer;
using ork.parser;
using ork.tokens;

internal static class OrkDriver
{
    private static void Main()
    {
        Console.WriteLine($"Hello {Environment.UserName}! This is the ORK programming language!");
        Console.WriteLine("Feel free to type in commands");
        Start();
    }

    private static void Start()
    {
        do
        {
            Console.Write(">> ");

            var line = Console.ReadLine();
            if (line is null)
            {
                break;
            }

            var lexer = new Lexer(line);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            if (parser.Errors.Count != 0)
            {
                foreach (var msg in parser.Errors)
                    Console.WriteLine(msg);
                continue;
            }
            Console.WriteLine(program);
        } while (true);
    }
}
using ork.lexer;
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
            Token token;
            do
            {
                token = lexer.NextToken();
                Console.WriteLine(token.ToString());
            } while (token.Tag != TokenTag.Eof);
        } while (true);
    }
}
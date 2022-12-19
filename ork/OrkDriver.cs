using ork.lexer;
using ork.tokens;
using System;

internal class OrkDriver
{
    private static void Main(string[] args)
    {
        Console.WriteLine($"Hello {System.Environment.UserName}! This is the ORK programming language!");
        Console.WriteLine("Feel free to type in commands");
        Start();
    }

    private static void Start()
    {
        do
        {
            Console.Write(">> ");

            var line = Console.ReadLine();
            if (line == null)
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
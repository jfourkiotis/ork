using BenchmarkDotNet.Attributes;
using ork.lexer;
using ork.tokens;
using System.Text;

namespace ork.benchmarks
{
    public interface IGenRandomToken
    {
        public string Generate();
    }

    public class RIdent : IGenRandomToken
    {
        private static Random random = new Random();
        private readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public string Generate()
        {
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
    public class RKeyword : IGenRandomToken
    {
        private static Random random = new Random();
        private readonly string[] keywords =
        {
            "fn", "let", "true", "false", "return", "if", "else",
        };
        public string Generate()
        {
            return keywords[random.Next(keywords.Length)];
        }
    }

    public class ROperator : IGenRandomToken
    {
        private static Random random = new Random();
        private readonly string[] operators =
        {
            "+", "-", "*", "/", "!", "<", ">", "==", "!=", "="
        };
        public string Generate()
        {
            return operators[random.Next(operators.Length)];
        }
    }

    public class RNumber : IGenRandomToken
    {
        private static Random random = new Random();
        public string Generate()
        {
            return random.Next(10).ToString();
        }
    }
    public class RPunct : IGenRandomToken
    {
        private static Random random = new Random();
        private readonly string[] chars =
        {
            "(", ")", "{", "}", ";", ",", 
        };
        public string Generate()
        {
            return chars[random.Next(chars.Length)];
        }
    }

    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    public class LexerBenchmarks
    {
        private readonly string input;
        public LexerBenchmarks()
        {
            input = GetRandomInput();
        }
        private string GetRandomInput()
        {
            Random random = new Random(14);
            IGenRandomToken[] randomTokens = 
            {
                new RIdent(),
                new RKeyword(),
                new ROperator(),
                new RNumber(), 
                new RPunct()
            };

            StringBuilder sb = new();
            // simulating a file with 2000 lines and 20 tokens each line
            for (int i = 0; i < 2000; ++i)
            {
                for (int j = 0; j < 19; ++j)
                {
                    sb.Append(randomTokens[random.Next(randomTokens.Length)].Generate());
                    sb.Append(' ');
                }
                sb.Append(';');
                sb.Append('\n');
            }
            return sb.ToString();
        }
        [Benchmark]
        public void Run()
        {
            var lexer = new Lexer(input);
            Token token;
            do
            {
                token = lexer.NextToken();
            } while (token.Tag != TokenTag.Eof);
        }
    }
}

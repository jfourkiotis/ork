using ork.tokens;
using ork.lexer;

namespace ork.tests
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TestNextTokenSimple()
        {
            const string input = "=+(){},;[]:";

            var tests = new[]
            {
                new { Tag = TokenTag.Assign, ExpectedLiteral = "="},
                new { Tag = TokenTag.Plus, ExpectedLiteral = "+" },
                new { Tag = TokenTag.LParen, ExpectedLiteral = "(" },
                new { Tag = TokenTag.RParen, ExpectedLiteral = ")" },
                new { Tag = TokenTag.LBrace, ExpectedLiteral = "{" },
                new { Tag = TokenTag.RBrace, ExpectedLiteral = "}" },
                new { Tag = TokenTag.Comma, ExpectedLiteral = "," },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.LBracket, ExpectedLiteral = "[" },
                new { Tag = TokenTag.RBracket, ExpectedLiteral = "]" },
                new { Tag = TokenTag.Colon, ExpectedLiteral = ":" },
                new { Tag = TokenTag.Eof, ExpectedLiteral = "" },
            };

            Lexer lexer = new Lexer(input);

            foreach (var test in tests)
            {
                var tok = lexer.NextToken();
                Assert.AreEqual(test.Tag, tok.Tag);
                Assert.AreEqual(test.ExpectedLiteral, tok.Literal);
            }
        }

        [TestMethod]
        public void TestComments()
        {
            string input = "# hello world";
            var lexer = new Lexer(input);
            Assert.AreEqual(TokenTag.Eof, lexer.NextToken().Tag);
        }
        
        [TestMethod]
        public void TestCommentsWithSourceCode()
        {
            string input = """
                #
                # number of dogs
                let y = 10;
            """;
            var tests = new[]
            {
                new { Tag = TokenTag.Let, ExpectedLiteral = "let" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "y" },
                new { Tag = TokenTag.Assign, ExpectedLiteral = "=" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "10" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
            };
            
            var lexer = new Lexer(input);
            foreach (var test in tests)
            {
                var tok = lexer.NextToken();
                Assert.AreEqual(test.Tag, tok.Tag);
                Assert.AreEqual(test.ExpectedLiteral, tok.Literal);
            }
        }

        [TestMethod]
        public void TestNextTokenComplex()
        {
            string input = """
                # five is an integer
                let five = 5; # it is greater than 4 but less than 6
                # ten is another integer
                let ten = 10;

                let add = fn(x, y) {
                    x + y; # what a nice function
                };

                let result = add(five, ten); # yeap

                !-/*5;
                5 < 10 > 5;

                if (5 < 10) {
                    # yes, this is correct
                    return true;
                } else {
                    # this is also correct
                    return false; # 123 but I don't like false
                }

                    10 == 10; 
                10 != 9;
                "foobar"
                "foo bar"
                [1, 2];
                {"foo": "bar"};
                """;

            var tests = new[]
            {
                new { Tag = TokenTag.Let, ExpectedLiteral = "let" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "five" },
                new { Tag = TokenTag.Assign, ExpectedLiteral = "=" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "5" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.Let, ExpectedLiteral = "let" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "ten" },
                new { Tag = TokenTag.Assign, ExpectedLiteral = "=" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "10" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.Let, ExpectedLiteral = "let" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "add" },
                new { Tag = TokenTag.Assign, ExpectedLiteral = "=" },
                new { Tag = TokenTag.Function, ExpectedLiteral = "fn" },
                new { Tag = TokenTag.LParen, ExpectedLiteral = "(" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "x" },
                new { Tag = TokenTag.Comma, ExpectedLiteral = "," },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "y" },
                new { Tag = TokenTag.RParen, ExpectedLiteral = ")" },
                new { Tag = TokenTag.LBrace, ExpectedLiteral = "{" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "x" },
                new { Tag = TokenTag.Plus, ExpectedLiteral = "+" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "y" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.RBrace, ExpectedLiteral = "}" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.Let, ExpectedLiteral = "let" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "result" },
                new { Tag = TokenTag.Assign, ExpectedLiteral = "=" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "add" },
                new { Tag = TokenTag.LParen, ExpectedLiteral = "(" },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "five" },
                new { Tag = TokenTag.Comma, ExpectedLiteral = "," },
                new { Tag = TokenTag.Ident, ExpectedLiteral = "ten" },
                new { Tag = TokenTag.RParen, ExpectedLiteral = ")" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.Bang, ExpectedLiteral = "!" },
                new { Tag = TokenTag.Minus, ExpectedLiteral = "-" },
                new { Tag = TokenTag.Slash, ExpectedLiteral = "/" },
                new { Tag = TokenTag.Asterisk, ExpectedLiteral = "*" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "5" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.Int, ExpectedLiteral = "5" },
                new { Tag = TokenTag.LessThan, ExpectedLiteral = "<"},
                new { Tag = TokenTag.Int, ExpectedLiteral = "10" },
                new { Tag = TokenTag.GreaterThan, ExpectedLiteral = ">"},
                new { Tag = TokenTag.Int, ExpectedLiteral = "5" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },

                new { Tag = TokenTag.If, ExpectedLiteral = "if" },
                new { Tag = TokenTag.LParen, ExpectedLiteral = "(" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "5" },
                new { Tag = TokenTag.LessThan, ExpectedLiteral = "<"},
                new { Tag = TokenTag.Int, ExpectedLiteral = "10" },
                new { Tag = TokenTag.RParen, ExpectedLiteral = ")" },
                new { Tag = TokenTag.LBrace, ExpectedLiteral = "{" },
                new { Tag = TokenTag.Return, ExpectedLiteral = "return" },
                new { Tag = TokenTag.True, ExpectedLiteral = "true" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.RBrace, ExpectedLiteral = "}" },
                new { Tag = TokenTag.Else, ExpectedLiteral = "else" },
                new { Tag = TokenTag.LBrace, ExpectedLiteral = "{" },
                new { Tag = TokenTag.Return, ExpectedLiteral = "return" },
                new { Tag = TokenTag.False, ExpectedLiteral = "false" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.RBrace, ExpectedLiteral = "}" },

                new { Tag = TokenTag.Int, ExpectedLiteral = "10" },
                new { Tag = TokenTag.Eq, ExpectedLiteral = "==" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "10"},
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "10"},
                new { Tag = TokenTag.NotEq, ExpectedLiteral = "!=" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "9"},
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.String, ExpectedLiteral = "foobar" },
                new { Tag = TokenTag.String, ExpectedLiteral = "foo bar" },
                new { Tag = TokenTag.LBracket, ExpectedLiteral = "[" },
                new { Tag = TokenTag.Int, ExpectedLiteral = "1" },
                new { Tag = TokenTag.Comma, ExpectedLiteral = "," },
                new { Tag = TokenTag.Int, ExpectedLiteral = "2" },
                new { Tag = TokenTag.RBracket, ExpectedLiteral = "]" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
                new { Tag = TokenTag.LBrace, ExpectedLiteral = "{" },
                new { Tag = TokenTag.String, ExpectedLiteral = "foo" },
                new { Tag = TokenTag.Colon, ExpectedLiteral = ":" },
                new { Tag = TokenTag.String, ExpectedLiteral = "bar" },
                new { Tag = TokenTag.RBrace, ExpectedLiteral = "}" },
                new { Tag = TokenTag.Semicolon, ExpectedLiteral = ";" },
            };

            Lexer lexer = new Lexer(input);

            foreach (var test in tests)
            {
                var tok = lexer.NextToken();
                Assert.AreEqual(test.Tag, tok.Tag);
                Assert.AreEqual(test.ExpectedLiteral, tok.Literal);
            }
        }
    }
}
using ork.lexer;
using ork.ast;
using ork.parser;

namespace ork.tests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestLetStatements()
        {
            string input = """
                let x = 5;
                let y = 10;
                let foobar = 838383;
                """;

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(3, program.Statements.Count);

            string[] tests = { "x", "y", "foobar" };
            foreach (var (first, second) in tests.Zip(program.Statements)) 
            {
                Assert.AreEqual("let", second.TokenLiteral);
                
                LetStatement? ls = second as LetStatement;
                Assert.IsNotNull(ls);
                Assert.AreEqual(first, ls.Name.TokenLiteral);
            }
        }

        [TestMethod]
        public void TestReturnStatements()
        {
            string input = """
                return 5;
                return 10;
                return 993322;
                """;

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(3, program.Statements.Count);

            foreach (var stmt in program.Statements)
            {
                ReturnStatement? rs = stmt as ReturnStatement;
                Assert.IsNotNull(rs);
                Assert.AreEqual("return", rs.TokenLiteral);
            }
        }

        [TestMethod]
        public void TestIdentifierExpressions() 
        {
            string input = "foobar;";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);  
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);
            
            Identifier? id = es.Expression as Identifier;
            Assert.IsNotNull(id);

            Assert.AreEqual("foobar", id.TokenLiteral);
        }

        [TestMethod]
        public void TestIntegerLiteralExpressions()
        {
            string input = "5;";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            IntegerLiteral? id = es.Expression as IntegerLiteral;
            Assert.IsNotNull(id);

            Assert.AreEqual("5", id.TokenLiteral);
        }

        [TestMethod]
        public void TestPrefixExpressions()
        {
            var tests = new[]
            {
                new {Input = "!5;", Operator = "!", Literal = "5"},
                new {Input = "-5;", Operator = "-", Literal = "5"},
            };

            foreach (var test in tests)
            {
                var lexer = new Lexer(test.Input);
                var parser = new Parser(lexer);
                var program = parser.ParseProgram();
                Assert.AreEqual(0, parser.Errors.Count);
                Assert.IsNotNull(program);
                Assert.AreEqual(1, program.Statements.Count);

                ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
                Assert.IsNotNull(es);

                PrefixExpression? num = es.Expression as PrefixExpression;
                Assert.IsNotNull(num);
            }
        }
    }
}

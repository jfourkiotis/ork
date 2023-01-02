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

        static void TestIntegerLiteral(IExpression? e, Int64 expectedValue)
        {
            Assert.IsNotNull(e);
            IntegerLiteral? num = e as IntegerLiteral;
            Assert.IsNotNull(num);
            Assert.AreEqual(expectedValue.ToString(), num.TokenLiteral);
            Assert.AreEqual(expectedValue, num.Value);
        }
        
        static void TestBooleanLiteral(IExpression? e, bool expectedValue)
        {
            Assert.IsNotNull(e);
            switch (expectedValue)
            {
                case true:
                {
                    TrueLiteral? t = e as TrueLiteral;
                    Assert.IsNotNull(t);
                    Assert.AreEqual("true", t.TokenLiteral);
                    break;
                }
                default:
                {
                    FalseLiteral? t = e as FalseLiteral;
                    Assert.IsNotNull(t);
                    Assert.AreEqual("false", t.TokenLiteral);
                    break;
                }
            }
        }

        static void TestIdentifier(IExpression? e, string id)
        {
            Assert.IsNotNull(e);
            Identifier? i = e as Identifier;
            Assert.IsNotNull(i);
            Assert.AreEqual(id, i.TokenLiteral);
            Assert.AreEqual(id, i.ToString());
        }

        static void TestLiteral(IExpression? e, object expectedValue)
        {
            switch (expectedValue)
            {
                case long value:
                    TestIntegerLiteral(e, value);
                    break;
                case bool value:
                    TestBooleanLiteral(e, value);
                    break;
                case string id:
                    TestIdentifier(e, id);
                    break;
                default:
                    Assert.Fail("unexpected value");
                    break;
            }
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

            TestIntegerLiteral(es.Expression, 5);
        }

        [TestMethod]
        public void TestPrefixExpressions()
        {
            var tests = new[]
            {
                new {Input = "!5;", Operator = "!", Rhs = 5},
                new {Input = "-5;", Operator = "-", Rhs = 5},
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
                Assert.AreEqual(test.Operator, num.TokenLiteral);
                TestIntegerLiteral(num.Rhs, test.Rhs);
            }
        }

        struct InfixTest
        {
            public InfixTest(string input, object lhs, string op, object rhs)
            {
                Input = input;
                Lhs = lhs;
                Operator = op;
                Rhs = rhs;
            }
            
            internal string Input { get;  }
            internal object Lhs { get; }
            internal string Operator { get; }
            internal object Rhs { get; }
        }

        [TestMethod]
        public void TestInfixExpressions()
        {
            var tests = new InfixTest[]
            {
                new(input: "5 + 5;", lhs: 5L, op: "+", rhs: 5L),
                new(input: "5 - 5;", lhs: 5L, op: "-", rhs: 5L),
                new(input: "5 * 5;", lhs: 5L, op: "*", rhs: 5L),
                new(input: "5 / 5;", lhs: 5L, op: "/", rhs: 5L),
                new(input: "5 > 5;", lhs: 5L, op: ">", rhs: 5L),
                new(input: "5 < 5;", lhs: 5L, op: "<", rhs: 5L),
                new(input: "5 == 5;", lhs: 5L, op: "==", rhs: 5L),
                new(input: "5 != 5;", lhs: 5L, op: "!=", rhs: 5L),
                new(input: "true == true", lhs: true, op: "==", rhs: true),
                new(input: "true != false", lhs: true, op: "!=", rhs: false),
                new(input: "false == false", lhs: false, op: "==", rhs: false),
                new(input: "alice * bob", lhs: "alice", op: "*", rhs: "bob"),
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

                InfixExpression? ie = es.Expression as InfixExpression;
                Assert.IsNotNull(ie);
                Assert.AreEqual(test.Operator, ie.TokenLiteral);
                TestLiteral(ie.Lhs, test.Lhs);
                TestLiteral(ie.Rhs, test.Rhs);
            }
        }

        [TestMethod]
        public void TestOperatorPrecedence()
        {
            var tests = new[]
            {
                new { Input = "-a * b", Expected = "((-a) * b)" },
                new { Input = "!-a", Expected = "(!(-a))" },
                new { Input = "a + b + c", Expected = "((a + b) + c)" },
                new { Input = "a + b - c", Expected = "((a + b) - c)" },
                new { Input = "a * b * c", Expected = "((a * b) * c)" },
                new { Input = "a * b / c", Expected = "((a * b) / c)" },
                new { Input = "a + b / c", Expected = "(a + (b / c))" },
                new { Input = "a + b * c + d / e - f", Expected = "(((a + (b * c)) + (d / e)) - f)" },
                new { Input = "3 + 4; -5 * 5", Expected = "(3 + 4)((-5) * 5)" },
                new { Input = "5 > 4 == 3 < 4", Expected = "((5 > 4) == (3 < 4))" },
                new { Input = "5 < 4 != 3 > 4", Expected = "((5 < 4) != (3 > 4))" },
                new { Input = "3 + 4 * 5 == 3 * 1 + 4 * 5", Expected = "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))" },
                new { Input = "true", Expected = "true" },
                new { Input = "false", Expected = "false" },
                new { Input = "3 > 5 == false", Expected = "((3 > 5) == false)" },
                new { Input = "3 < 5 == true", Expected = "((3 < 5) == true)" },
                new { Input = "(5 + 5) * 2", Expected = "((5 + 5) * 2)" },
                new { Input = "-(5 + 5)", Expected = "(-(5 + 5))" },
                new { Input = "!(true == true)", Expected = "(!(true == true))" },
            };

            foreach (var test in tests)
            {
                var lexer = new Lexer(test.Input);
                var parser = new Parser(lexer);
                var program = parser.ParseProgram();
                Assert.AreEqual(0, parser.Errors.Count);
                Assert.IsNotNull(program);
                Assert.AreEqual(test.Expected, program.ToString());
            }
        }
    }
}

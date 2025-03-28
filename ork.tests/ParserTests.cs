﻿using ork.lexer;
using ork.ast;
using ork.parser;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

namespace ork.tests
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void TestLetStatements()
        {
            var tests = new[]
            {
                new { Input = "let x = 5;", ExpectedId = "x", ExpectedValue = (object)5L },
                new { Input = "let y = true;", ExpectedId = "y", ExpectedValue = (object)true },
                new { Input = "let foobar = y;", ExpectedId = "foobar", ExpectedValue = (object)"y" },
            };

            foreach (var test in tests)
            {

                var lexer = new Lexer(test.Input);
                var parser = new Parser(lexer);
                var program = parser.ParseProgram();
                Assert.AreEqual(0, parser.Errors.Count);
                Assert.IsNotNull(program);
                Assert.AreEqual(1, program.Statements.Count);

                LetStatement? ls = program.Statements[0] as LetStatement;
                Assert.IsNotNull(ls);

                TestIdentifier(ls.Name, test.ExpectedId);
                TestLiteral(ls.Expression, test.ExpectedValue);
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

        static void TestIntegerLiteral(Expression? e, Int64 expectedValue)
        {
            Assert.IsNotNull(e);
            IntegerLiteral? num = e as IntegerLiteral;
            Assert.IsNotNull(num);
            Assert.AreEqual(expectedValue.ToString(), num.TokenLiteral);
            Assert.AreEqual(expectedValue, num.Value);
        }
        
        static void TestBooleanLiteral(Expression? e, bool expectedValue)
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

        static void TestIdentifier(Expression? e, string id)
        {
            Assert.IsNotNull(e);
            Identifier? i = e as Identifier;
            Assert.IsNotNull(i);
            Assert.AreEqual(id, i.TokenLiteral);
            Assert.AreEqual(id, i.ToString());
        }

        static void TestLiteral(Expression? e, object expectedValue)
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

        void TestInfixExpression(Expression? e, object lhs, string op, object rhs)
        {
            InfixExpression? ie = e as InfixExpression;
            Assert.IsNotNull(ie);
            Assert.AreEqual(op, ie.TokenLiteral);
            TestLiteral(ie.Lhs, lhs);
            TestLiteral(ie.Rhs, rhs);
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
                TestInfixExpression(es.Expression, test.Lhs, test.Operator, test.Rhs);
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
                new { Input = "a + add(b * c) + d", Expected = "((a + add((b * c))) + d)" },
                new { Input = "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", Expected = "add(a,b,1,(2 * 3),(4 + 5),add(6,(7 * 8)))" },
                new { Input = "add(a + b + c * d / f + g)", Expected = "add((((a + b) + ((c * d) / f)) + g))" },
                new { Input = "a * [1, 2, 3, 4][b * c] * d", Expected = "((a * ([1,2,3,4][(b * c)])) * d)"},
                new { Input = "add(a * b[2], b[1], 2 * [1, 2][1])", Expected = "add((a * (b[2])),(b[1]),(2 * ([1,2][1])))" },
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

        [TestMethod]
        public void TestIfExpression()
        {
            string input = "if (x < y) { x }";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);
            
            IfExpression? ie = es.Expression as IfExpression;
            Assert.IsNotNull(ie);
            TestInfixExpression(ie.Condition, "x", "<", "y");
            
            Assert.AreEqual(1, ie.Then.Statements.Count);

            ExpressionStatement? then0 = ie.Then.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(then0);
            TestIdentifier(then0.Expression, "x");
            Assert.IsNull(ie.Else);
        }
        
        [TestMethod]
        public void TestIfElseExpression()
        {
            string input = "if (x < y) { x } else { y }";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);
            
            IfExpression? ie = es.Expression as IfExpression;
            Assert.IsNotNull(ie);
            TestInfixExpression(ie.Condition, "x", "<", "y");
            
            Assert.IsNotNull(ie.Then);
            Assert.AreEqual(1, ie.Then.Statements.Count);

            ExpressionStatement? then0 = ie.Then.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(then0);
            TestIdentifier(then0.Expression, "x");
            
            Assert.IsNotNull(ie.Else);
            Assert.AreEqual(1, ie.Else.Statements.Count);
            ExpressionStatement? else0 = ie.Else.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(else0);
            TestIdentifier(else0.Expression, "y");
        }

        [TestMethod]
        public void TestFunctionLiteralParsing()
        {
            string input = "fn(x, y) { x + y; }";
            
            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);
            
            FunctionLiteral? fl = es.Expression as FunctionLiteral;
            Assert.IsNotNull(fl);
            Assert.AreEqual(2, fl.Parameters.Count);
            TestLiteral(fl.Parameters[0], "x");
            TestLiteral(fl.Parameters[1], "y");
            
            Assert.AreEqual(1, fl.Body.Statements.Count);
            ExpressionStatement? esb = fl.Body.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(esb);
            TestInfixExpression(esb.Expression, "x", "+", "y");
        }

        [TestMethod]
        public void TestFunctionParameterParsing()
        {
            var tests = new[]
            {
                new { Input = "fn(){}", Expected = Array.Empty<string>() },
                new { Input = "fn(x){}", Expected = new[]{ "x" } }, 
                new { Input = "fn(x, y, z){}", Expected = new[] { "x", "y", "z"} },
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
                
                FunctionLiteral? fl = es.Expression as FunctionLiteral;
                Assert.IsNotNull(fl);

                Assert.IsTrue(test.Expected.SequenceEqual(fl.Parameters.Select(p => p.ToString())));
            }
        }

        [TestMethod]
        public void TestCallExpressionParsing()
        {
            string input = "add(1, 2 * 3, 4 + 5);";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            CallExpression? fl = es.Expression as CallExpression;
            Assert.IsNotNull(fl);

            TestIdentifier(fl.Function, "add");
            Assert.AreEqual(3, fl.Arguments.Count);
            TestLiteral(fl.Arguments[0], 1L);
            TestInfixExpression(fl.Arguments[1], 2L, "*", 3L);
            TestInfixExpression(fl.Arguments[2], 4L, "+", 5L); 
        }

        [TestMethod]
        public void TestParsingArrayLiterals()
        {
            string input = "[1, 2*2, 3 + 3]";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            ArrayLiteral? al = es.Expression as ArrayLiteral;
            Assert.IsNotNull(al);
            Assert.AreEqual(3, al.Elements.Count);
            TestIntegerLiteral(al.Elements[0], 1L);
            TestInfixExpression(al.Elements[1], 2L, "*", 2L);
            TestInfixExpression(al.Elements[2], 3L, "+", 3L);
        }

        [TestMethod]
        public void TestParsingIndexExpressions()
        {
            string input = "myArray[1+1]";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            IndexExpression? ie = es.Expression as IndexExpression;
            Assert.IsNotNull(ie);
            TestIdentifier(ie.Left, "myArray");
            TestInfixExpression(ie.Index, 1L, "+", 1L);
        }

        [TestMethod]
        public void TestParsingEmptyHashLiterals()
        {
            string input = "{}";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            HashLiteral? hl = es.Expression as HashLiteral;
            Assert.IsNotNull(hl);
            Assert.AreEqual(0, hl.Pairs.Count);
        }

        [TestMethod]
        public void TestParsingHashLiteralsStringKeys()
        {
            string input = """{"one": 1, "two": 2, "three": 3}""";

            var lexer = new Lexer(input);
            var parser = new Parser(lexer);
            var program = parser.ParseProgram();
            Assert.AreEqual(0, parser.Errors.Count);
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.Statements.Count);

            ExpressionStatement? es = program.Statements[0] as ExpressionStatement;
            Assert.IsNotNull(es);

            HashLiteral? hl = es.Expression as HashLiteral;
            Assert.IsNotNull(hl);
            Assert.AreEqual(3, hl.Pairs.Count);

            var expected = new Dictionary<string, long>()
            {
                {"one", 1L}, {"two", 2L }, {"three", 3L },
            };
            foreach (var (k, v) in hl.Pairs)
            {
                StringLiteral? sl = k as StringLiteral;
                Assert.IsNotNull(sl);
                if (!expected.TryGetValue(sl.ToString(), out var expectedValue))
                {
                    Assert.Fail($"key: {sl.ToString()} not found");
                }
                TestIntegerLiteral(v, expectedValue);
            }
        }
    }
}

using ork.lexer;
using ork.parser;

namespace ork.tests;

[TestClass]
public class TreeWalkingInterpreterTest
{
    static object? TestEval(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.ParseProgram();
        var tw = new TreeWalkingInterpreter();
        return tw.Eval(program);
    }
    [TestMethod]
    public void TestIntegerExpressions()
    {
        var tests = new[]
        {
            new { Input = "5", Output = 5L },
            new { Input = "10", Output = 10L },
            new { Input = "-5", Output = -5L },
            new { Input = "-10", Output = -10L },
            new { Input = "5 + 5 + 5 + 5 - 10", Output = 10L },
            new { Input = "2 * 2 * 2 * 2 * 2", Output = 32L },
            new { Input = "-50 + 100 + -50", Output = 0L },
            new { Input = "5 * 2 + 10", Output = 20L },
            new { Input = "5 + 2 * 10", Output = 25L },
            new { Input = "20 + 2 * -10", Output = 0L },
            new { Input = "50 / 2 * 2 + 10", Output = 60L },
            new { Input = "2 * (5 + 10)", Output = 30L },
            new { Input = "3 * 3 * 3 + 10", Output = 37L },
            new { Input = "3 * (3 * 3) + 10", Output = 37L },
            new { Input = "(5 + 10 * 2 + 15 / 3) * 2 + -10", Output = 50L },
        };
        foreach (var test in tests)
        {
            var result = TestEval(test.Input);
            Assert.AreEqual(test.Output, result);
        }
    }
    [TestMethod]
    public void TestBooleanExpressions()
    {
        var tests = new[]
        {
            new { Input = "true", Output = true },
            new { Input = "false", Output = false },
            new { Input = "1 < 2", Output = true},
            new { Input = "1 > 2", Output = false},
            new { Input = "1 < 1", Output = false},
            new { Input = "1 > 1", Output = false},
            new { Input = "1 == 1", Output = true},
            new { Input = "1 != 1", Output = false},
            new { Input = "1 == 2", Output = false},
            new { Input = "1 != 2", Output = true},
            new { Input = "true == true", Output = true},
            new { Input = "false == false", Output = true},
            new { Input = "true == false", Output = false},
            new { Input = "true != false", Output = true},
            new { Input = "false != true", Output = true},
            new { Input = "(1 < 2) == true", Output = true},
            new { Input = "(1 < 2) == false", Output = false},
            new { Input = "(1 > 2) == true", Output = false},
            new { Input = "(1 > 2) == false", Output = true},
        };
        foreach (var test in tests)
        {
            var result = TestEval(test.Input);
            Assert.AreEqual(test.Output, result);
        }
    }

    [TestMethod]
    public void TestBangOperator()
    {
        var tests = new[]
        {
            ("!!true", true),
            ("!!false", false),
            ("!true", false),
            ("!false", true),
            ("!5", false),
            ("!!5", true),
        };

        foreach (var (input, expected) in tests)
        {
            var result = TestEval(input);
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, (bool)result, input);
        }
    }

    [TestMethod]
    public void TestIfElseExpression()
    {
        var tests = new[]
        {
            ("if (true) { 10 }", (object)10L),
            ("if (false) { 10 }", null),
            ("if (1) { 10 }", (object)10L),
            ("if (1 < 2) { 10 }", (object)10L),
            ("if (1 > 2) { 10 }", null),
            ("if (1 > 2) { 10 } else { 20 }", (object)20L),
            ("if (1 < 2) { 10 } else { 20 }", (object)10L),
        };

        foreach (var (input, expected) in tests)
        {
            var result = TestEval(input);
            Assert.AreEqual(expected, result);
        }
    }
    
    [TestMethod]
    public void TestReturnStatement()
    {
        var tests = new[]
        {
            ("return 10;", 10L),
            ("return 10; 9;", 10L),
            ("return 2 * 5; 9;", 10L),
            ("9; return 2 * 5; 9;", 10L),
            ("if (10 > 1) { if (10 > 1) { return 10; } return 1 }", 10L),
        };

        foreach (var (input, expected) in tests)
        {
            var result = TestEval(input);
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, (Int64)result);
        }
    }

    [TestMethod]
    public void TestErrorHandling()
    {
        var tests = new[]
        {
            ("5 + true;", "type mismatch: INTEGER + BOOLEAN"),
            ("5 + true; 5;", "type mismatch: INTEGER + BOOLEAN"),
            ("-true", "unknown operator: -BOOLEAN"),
            ("true + false", "unknown operator: BOOLEAN + BOOLEAN"),
            ("if (10 > 1) { true + false ; }", "unknown operator: BOOLEAN + BOOLEAN"),
        };

        foreach (var (input, expected) in tests)
        {
            var ex = Assert.ThrowsException<OrkRuntimeException>(() => TestEval(input));
            Assert.AreEqual(expected, ex.Message);
        }
    }
}
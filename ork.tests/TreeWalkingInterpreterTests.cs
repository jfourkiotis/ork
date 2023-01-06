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
}
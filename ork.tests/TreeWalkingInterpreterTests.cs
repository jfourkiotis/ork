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
    public void TestIntegerExpression()
    {
        var tests = new[]
        {
            new { Input = "5", Output = 5L },
            new { Input = "10", Output = 10L },
        };
        foreach (var test in tests)
        {
            var result = TestEval(test.Input);
            Assert.AreEqual(test.Output, result);
        }
    }
}
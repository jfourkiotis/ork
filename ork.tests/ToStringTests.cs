using ork.ast;

namespace ork.tests
{
    [TestClass]
    public class ToStringTests
    {
        [TestMethod]
        public void TestProgramToString()
        {
            var program = new Program(new List<Statement>()
            {
                new LetStatement(new tokens.Token(tokens.TokenTag.Let, "let", 0, 0), new Identifier(new tokens.Token(tokens.TokenTag.Ident, "myVar", 0, 0)), new Identifier(new tokens.Token(tokens.TokenTag.Ident, "anotherVar", 0, 0))),
            });
            Assert.AreEqual("let myVar = anotherVar;", program.ToString());
        }
    }
}

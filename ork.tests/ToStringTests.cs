using ork.ast;
using System.Collections.Generic;

namespace ork.tests
{
    [TestClass]
    public class ToStringTests
    {
        [TestMethod]
        public void TestProgramToString()
        {
            var program = new Program(new List<IStatement>()
            {
                new LetStatement(new tokens.Token(tokens.TokenTag.Let, "let"), new Identifier(new tokens.Token(tokens.TokenTag.Ident, "myVar")), new Identifier(new tokens.Token(tokens.TokenTag.Ident, "anotherVar"))),
            });
            Assert.AreEqual("let myVar = anotherVar;", program.ToString());
        }
    }
}

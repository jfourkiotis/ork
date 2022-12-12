using ork.tokens;

namespace ork.tests
{
    [TestClass]
    public class TokenTagTest
    {
        [TestMethod]
        public void TestTokenTagName()
        {
            TokenTag tag = TokenTag.Let;
            Assert.AreEqual("let", tag.Name());
        }
    }
}
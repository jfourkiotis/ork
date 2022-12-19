﻿using ork.lexer;
using ork.tokens;
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
            foreach (var (First, Second) in tests.Zip(program.Statements)) 
            {
                Assert.AreEqual("let", Second.TokenLiteral);
                
                LetStatement? ls = Second as LetStatement;
                Assert.IsNotNull(ls);
                Assert.AreEqual(First, ls.Name.TokenLiteral);
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
    }
}

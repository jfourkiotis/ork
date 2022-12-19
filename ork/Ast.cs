using System;
using System.Collections.Generic;
using System.Text;

using ork.tokens;

namespace ork.ast
{
    public interface INode
    {
        public abstract string TokenLiteral { get; }
    }

    public interface IStatement : INode
    {
    }

    public interface IExpression : INode
    {
    }

    public sealed class Program : INode
    {
        public Program(IList<IStatement> statements)
        {
            Statements = statements.AsReadOnly();
        }

        public IReadOnlyList<IStatement> Statements { get; private set; }

        public string TokenLiteral
        {
            get
            {
                if (Statements.Count == 0)
                {
                    return "";
                } else
                {
                    return Statements[0].TokenLiteral;
                }
            }
        }
    }

    public sealed class Identifier : IExpression
    {
        private readonly Token token;

        public Identifier(Token token) {
            this.token = token;
        }

        public string TokenLiteral => token.Literal;
    }

    public sealed class LetStatement : IStatement
    {
        // TokenTag.Let
        private readonly Token token; 

        public LetStatement(Token token, Identifier name)
        {
            this.token = token;
            Name = name;
        }

        public Identifier Name { get; private set; }

        public string TokenLiteral => token.Literal;
    }

    public sealed class ReturnStatement : IStatement
    {
        // TokenTag.Return
        private readonly Token token;

        public ReturnStatement(Token token, IExpression? expression)
        {
            this.token = token;
            Expression = expression;
        }

        public IExpression? Expression { get; }

        public string TokenLiteral => token.Literal;
    }
}

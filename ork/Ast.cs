using System.Text;

using ork.tokens;

namespace ork.ast
{
    public interface INode
    {
        public string TokenLiteral { get; }
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

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var statement in Statements)
            {
                sb.Append(statement);
            }
            return sb.ToString();
        }
    }

    public sealed class ExpressionStatement : IStatement
    {
        private readonly Token token;
      
        public ExpressionStatement(Token token, IExpression? expression)
        {
            this.token = token;
            Expression = expression;
        }

        public string TokenLiteral => token.Literal;
        public IExpression? Expression { get; }


        public override string ToString() => Expression?.ToString() ?? "";
    }

    public sealed class Identifier : IExpression
    {
        private readonly Token token;

        public Identifier(Token token) {
            this.token = token;
        }

        public string TokenLiteral => token.Literal;

        public override string ToString()
        {
            return token.Literal;
        }
    }

    public sealed class LetStatement : IStatement
    {
        // TokenTag.Let
        private readonly Token token;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public LetStatement(Token token, Identifier name, IExpression expression)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.token = token;
            Name = name;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.\
            // FIXME
            Expression = expression;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        public Identifier Name { get; }
        public IExpression Expression { get; }

        public string TokenLiteral => token.Literal;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(token.Literal);
            sb.Append(' ');
            sb.Append(Name);
            sb.Append(" = ");

            if (Expression != null)
            {
                sb.Append(Expression);
            }

            sb.Append(';');
            return sb.ToString();
        }
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

        public override string? ToString()
        {
            StringBuilder sb = new();
            sb.Append(token.Literal);
            
            if (Expression != null) 
            {
                sb.Append(' ');
                sb.Append(Expression);
            }

            sb.Append(';');
            return base.ToString();
        }
    }

    public sealed class IntegerLiteral : IExpression
    {
        private readonly Token token;

        public IntegerLiteral(Token token, Int64 val)
        {
            this.token = token;
            Value = val;
        }

        public Int64 Value { get; }

        public string TokenLiteral => token.Literal;

        public override string ToString()
        {
            return token.Literal;
        }
    }

    public sealed class PrefixExpression : IExpression
    {
        private readonly Token token; // the prefix token, e.g "!"
        public PrefixExpression(Token token, IExpression rhs)
        {
            this.token = token;
            Rhs = rhs;
        }
        public string TokenLiteral => token.Literal;
        public IExpression Rhs { get; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('(').Append(token.Literal).Append(Rhs).Append(')');
            return sb.ToString();
        }
    }
}

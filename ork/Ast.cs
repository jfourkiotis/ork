using System.Collections.ObjectModel;
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

        public IReadOnlyList<IStatement> Statements { get; }

        public string TokenLiteral => Statements.Count == 0 ? "" : Statements[0].TokenLiteral;

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

            if (Expression is not null)
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
            
            if (Expression is not null) 
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
    
    public sealed class InfixExpression : IExpression
    {
        private readonly Token token; // the prefix token, e.g "!"
        public InfixExpression(Token token, IExpression lhs, IExpression rhs)
        {
            this.token = token;
            Lhs = lhs;
            Rhs = rhs;
        }
        public string TokenLiteral => token.Literal;
        public IExpression Lhs { get; }
        public IExpression Rhs { get; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('(');
            sb.Append(Lhs);
            sb.Append(' ');
            sb.Append(token.Literal);
            sb.Append(' ');
            sb.Append(Rhs);
            sb.Append(')');
            return sb.ToString();
        }
    }

    public sealed class TrueLiteral : IExpression
    {
        private readonly Token token;
        public TrueLiteral(Token token) => this.token = token;
        public string TokenLiteral => token.Literal;
        public override string ToString() => "true";
    }
    
    public sealed class FalseLiteral : IExpression
    {
        private readonly Token token;
        public FalseLiteral(Token token) => this.token = token;
        public string TokenLiteral => token.Literal;
        public override string ToString() => "false";
    }

    public sealed class BlockStatement : IStatement
    {
        private readonly Token token; // the '{' token

        public BlockStatement(Token token, IList<IStatement> statements)
        {
            this.token = token;
            Statements = statements.AsReadOnly();
        }
        public IReadOnlyList<IStatement> Statements { get;  }
        public string TokenLiteral => token.Literal;

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var s in Statements)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }
    }

    public sealed class IfExpression : IExpression
    {
        private readonly Token token; // 'if'

        public IfExpression(Token token, IExpression condition, BlockStatement thenStatement,
            BlockStatement? elseStatement)
        {
            this.token = token;
            Condition = condition;
            Then = thenStatement;
            Else = elseStatement;
        }

        public string TokenLiteral => token.Literal;
        public IExpression Condition { get; }
        public BlockStatement Then { get;  }
        public BlockStatement? Else { get; }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append("if");
            sb.Append(Condition);
            sb.Append(' ');
            sb.Append(Then);
            sb.Append("else ");
            sb.Append(Else);
            
            return sb.ToString();
        }
    }

    public sealed class FunctionLiteral : IExpression
    {
        private readonly Token token; // 'fn'

        public FunctionLiteral(Token token, IList<Identifier> parameters, BlockStatement body)
        {
            this.token = token;
            Parameters = parameters.AsReadOnly();
            Body = body;
        }
        public BlockStatement Body { get; }
        public ReadOnlyCollection<Identifier> Parameters { get; }
        public string TokenLiteral => token.Literal;
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append("fn");
            sb.Append('(');
            sb.Append(String.Join(',', Parameters));
            sb.Append(')');
            sb.Append(Body);
            
            return sb.ToString();
        }
    }

    public sealed class CallExpression : IExpression
    {
        private readonly Token token; // '('

        public CallExpression(Token token, IExpression function, IList<IExpression> arguments)
        {
            this.token = token;
            Function = function;
            Arguments = arguments.AsReadOnly();
        }

        public string TokenLiteral => token.Literal;
        public IExpression Function { get; }
        public IReadOnlyList<IExpression> Arguments { get; }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append(Function);
            sb.Append('(');
            sb.Append(string.Join(',', Arguments));
            sb.Append(')');

            return sb.ToString();
        }
    }
}

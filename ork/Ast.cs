using System.Collections.ObjectModel;
using System.Text;

using ork.tokens;

namespace ork.ast
{
    public abstract class Node
    {
        public Node(Token token)
        {
            Token = token;
        }

        public Token Token { get; init; }
        public string TokenLiteral => Token.Literal;
    }

    public abstract class Statement : Node
    {
        protected Statement(Token token) : base(token)
        {
        }
    }

    public abstract class Expression : Node
    {
        protected Expression(Token token) : base(token)
        {
        }
    }

    // Program is not an AST node
    public sealed class Program
    {
        public Program(IList<Statement> statements)
        {
            Statements = statements.AsReadOnly();
        }

        public IReadOnlyList<Statement> Statements { get; }

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

    public sealed class ExpressionStatement : Statement
    {      
        public ExpressionStatement(Token token, Expression? expression) : base(token) 
        {
            Expression = expression;
        }

        public Expression? Expression { get; }

        public override string ToString() => Expression?.ToString() ?? "";
    }

    public sealed class Identifier : Expression
    {
        public Identifier(Token token) : base(token)
        {
        }

        public override string ToString()
        {
            return Token.Literal;
        }
    }

    public sealed class LetStatement : Statement
    {
        public LetStatement(Token token, Identifier name, Expression expression) : base(token)
        {
            Name = name;
            Expression = expression;
        }

        public Identifier Name { get; }
        public Expression Expression { get; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(Token.Literal);
            sb.Append(' ');
            sb.Append(Name);
            sb.Append(" = ");
            sb.Append(Expression);
            sb.Append(';');
            return sb.ToString();
        }
    }

    public sealed class ReturnStatement : Statement
    {
        public ReturnStatement(Token token, Expression? expression) : base(token)
        {
            Expression = expression;
        }

        public Expression? Expression { get; }
        public override string? ToString()
        {
            StringBuilder sb = new();
            sb.Append(Token.Literal);
            
            if (Expression is not null) 
            {
                sb.Append(' ');
                sb.Append(Expression);
            }

            sb.Append(';');
            return base.ToString();
        }
    }

    public sealed class IntegerLiteral : Expression
    {
        public IntegerLiteral(Token token, Int64 val) : base(token)
        {
            Value = val;
        }

        public Int64 Value { get; }
        public override string ToString()
        {
            return Token.Literal;
        }
    }

    public sealed class PrefixExpression : Expression
    {
        public PrefixExpression(Token token, Expression rhs) : base(token) 
        { 
            Rhs = rhs; 
        }
        public Expression Rhs { get; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('(').Append(Token.Literal).Append(Rhs).Append(')');
            return sb.ToString();
        }
    }
    
    public sealed class InfixExpression : Expression
    {
        public InfixExpression(Token token, Expression lhs, Expression rhs) : base(token)
        {
            Lhs = lhs;
            Rhs = rhs;
        }
        public Expression Lhs { get; }
        public Expression Rhs { get; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append('(');
            sb.Append(Lhs);
            sb.Append(' ');
            sb.Append(Token.Literal);
            sb.Append(' ');
            sb.Append(Rhs);
            sb.Append(')');
            return sb.ToString();
        }
    }

    public sealed class TrueLiteral : Expression
    {
        public TrueLiteral(Token token) : base(token)
        {

        }
        public override string ToString() => "true";
    }
    
    public sealed class FalseLiteral : Expression
    {
        public FalseLiteral(Token token) : base(token)
        {

        }
        public override string ToString() => "false";
    }

    public sealed class StringLiteral : Expression
    {
        public StringLiteral(Token token) : base(token)
        {

        }

        public override string ToString() => Token.Literal;
    }

    public sealed class BlockStatement : Statement
    {
        public BlockStatement(Token token, IList<Statement> statements) : base(token)
        {
            Statements = statements.AsReadOnly();
        }
        public IReadOnlyList<Statement> Statements { get;  }
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

    public sealed class IfExpression : Expression
    {
        public IfExpression(Token token, Expression condition, BlockStatement thenStatement,
            BlockStatement? elseStatement) : base(token)
        {
            Condition = condition;
            Then = thenStatement;
            Else = elseStatement;
        }

        public Expression Condition { get; }
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

    public sealed class FunctionLiteral : Expression
    {
        public FunctionLiteral(Token token, IList<Identifier> parameters, BlockStatement body) : base(token)
        {
            Parameters = parameters.AsReadOnly();
            Body = body;
        }
        public BlockStatement Body { get; }
        public ReadOnlyCollection<Identifier> Parameters { get; }
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

    public sealed class CallExpression : Expression
    {
        public CallExpression(Token token, Expression function, IList<Expression> arguments) : base(token)
        {
            Function = function;
            Arguments = arguments.AsReadOnly();
        }

        public Expression Function { get; }
        public IReadOnlyList<Expression> Arguments { get; }

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

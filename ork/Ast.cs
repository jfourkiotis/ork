﻿using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;

using ork.tokens;

namespace ork.ast
{
    public enum AstTag
    {
        ExpressionStatement,
        LetStatement,
        ReturnStatement,
        BlockStatement,
        Identifier,
        IntegerLiteral,
        TrueLiteral,
        FalseLiteral,
        StringLiteral,
        FunctionLiteral,
        ArrayLiteral,
        HashLiteral,
        PrefixExpression,
        InfixExpression,
        IfExpression,
        CallExpression,
        IndexExpression,
    }
    public abstract class Node
    {
        protected Node(Token token, AstTag tag)
        {
            Token = token;
            Tag = tag;
        }

        protected Token Token { get; }
        public AstTag Tag { get; }

        public string TokenLiteral => Token.Literal;
    }

    public abstract class Statement : Node
    {
        protected Statement(Token token, AstTag tag) : base(token, tag)
        {
        }
    }

    public abstract class Expression : Node
    {
        protected Expression(Token token, AstTag tag) : base(token, tag)
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
        public ExpressionStatement(Token token, Expression? expression) : base(token, AstTag.ExpressionStatement) 
        {
            Expression = expression;
        }

        public Expression? Expression { get; }

        public override string ToString() => Expression?.ToString() ?? "";
    }

    public sealed class Identifier : Expression
    {
        public Identifier(Token token) : base(token, AstTag.Identifier)
        {
        }

        public override string ToString()
        {
            return Token.Literal;
        }
    }

    public sealed class LetStatement : Statement
    {
        public LetStatement(Token token, Identifier name, Expression expression) : base(token, AstTag.LetStatement)
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
        public ReturnStatement(Token token, Expression? expression) : base(token, AstTag.ReturnStatement)
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
        public IntegerLiteral(Token token, Int64 val) : base(token, AstTag.IntegerLiteral)
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
        public PrefixExpression(Token token, Expression rhs) : base(token, AstTag.PrefixExpression) 
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
        public InfixExpression(Token token, Expression lhs, Expression rhs) : base(token, AstTag.InfixExpression)
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
        public TrueLiteral(Token token) : base(token, AstTag.TrueLiteral)
        {

        }
        public override string ToString() => "true";
    }
    
    public sealed class FalseLiteral : Expression
    {
        public FalseLiteral(Token token) : base(token, AstTag.FalseLiteral)
        {

        }
        public override string ToString() => "false";
    }

    public sealed class StringLiteral : Expression
    {
        public StringLiteral(Token token) : base(token, AstTag.StringLiteral)
        {

        }

        public override string ToString() => Token.Literal;
    }

    public sealed class BlockStatement : Statement
    {
        public BlockStatement(Token token, IList<Statement> statements) : base(token, AstTag.BlockStatement)
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
            BlockStatement? elseStatement) : base(token, AstTag.IfExpression)
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
        public FunctionLiteral(Token token, IList<Identifier> parameters, BlockStatement body) : base(token, AstTag.FunctionLiteral)
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
        public CallExpression(Token token, Expression function, IList<Expression> arguments) : base(token, AstTag.CallExpression)
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

    public sealed class ArrayLiteral : Expression
    {
        public ArrayLiteral(Token token, IList<Expression> elements) : base(token, AstTag.ArrayLiteral)
        {
            Elements = elements.AsReadOnly();
        }

        public ReadOnlyCollection<Expression> Elements { get; }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append('[');
            sb.Append(String.Join(',', Elements));
            sb.Append(']');

            return sb.ToString();
        }
    }

    public sealed class IndexExpression : Expression
    {
        public IndexExpression(Token token, Expression left, Expression index) : base(token, AstTag.IndexExpression)
        {
            Left = left;
            Index = index;
        }

        public Expression Left { get; }
        public Expression Index { get; }
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append('(');
            sb.Append(Left);
            sb.Append('[');
            sb.Append(Index);
            sb.Append(']');
            sb.Append(')');

            return sb.ToString();
        }
    }

    public sealed class HashLiteral : Expression
    {
        public HashLiteral(Token token, IDictionary<Expression, Expression?> pairs) : base(token, AstTag.HashLiteral) 
        {
            Pairs = pairs.ToImmutableDictionary();
        }

        public ImmutableDictionary<Expression, Expression?> Pairs { get; }
        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append('{');
            sb.Append(String.Join(',', Pairs.Select(p => p.Key + ":" + p.Value)));
            sb.Append('}');

            return sb.ToString();
        }
    }
}

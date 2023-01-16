using ork.ast;
using ork.lexer;
using ork.tokens;
using System.Runtime.CompilerServices;

namespace ork.parser
{
    using PrefixParseFn = Func<Parser, Expression?>;
    using InfixParseFn = Func<Parser, Expression, Expression?>;

    public enum Precedence
    {
        Lowest,
        Equals,
        LessGreater,
        Sum,
        Product,
        Prefix,
        Call, 
        Index,
    }

    public sealed class Parser
    {
        private readonly Lexer lexer;
        private Token curToken;
        private Token peekToken;
        private static readonly IDictionary<TokenTag, PrefixParseFn> PrefixParseFns =
            new Dictionary<TokenTag, PrefixParseFn>()
        {
            { TokenTag.Ident, p => p.ParseIdentifier() },
            { TokenTag.Int, p => p.ParseIntegerLiteral() },
            { TokenTag.Bang, p => p.ParsePrefixExpression() },
            { TokenTag.Minus, p => p.ParsePrefixExpression() },
            { TokenTag.True, p => p.ParseBooleanLiteral() },
            { TokenTag.False, p => p.ParseBooleanLiteral() },
            { TokenTag.LParen, p => p.ParseGroupedExpression() },
            { TokenTag.LBracket, p => p.ParseArrayLiteral() },
            { TokenTag.If, p => p.ParseIfExpression() },
            { TokenTag.Function, p => p.ParseFunctionLiteral() },
            { TokenTag.String, p => p.ParseStringLiteral() },
            { TokenTag.LBrace, p => p.ParseHashLiteral() },
        };
        private static readonly IDictionary<TokenTag, InfixParseFn> InfixParseFns =
            new Dictionary<TokenTag, InfixParseFn>()
        {
            { TokenTag.Plus, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.Minus, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.Slash, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.Asterisk, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.Eq, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.NotEq, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.LessThan, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.GreaterThan, (p, lhs) => p.ParseInfixExpression(lhs) },
            { TokenTag.LParen, (p, lhs) => p.ParseCallExpression(lhs) },
            { TokenTag.LBracket, (p, lhs) => p.ParseIndexExpression(lhs) },
        };

        private static readonly IDictionary<TokenTag, Precedence> Precedences = new Dictionary<TokenTag, Precedence>()
        {
            { TokenTag.Eq, Precedence.Equals },
            { TokenTag.NotEq, Precedence.Equals },
            { TokenTag.LessThan, Precedence.LessGreater },
            { TokenTag.GreaterThan, Precedence.LessGreater },
            { TokenTag.Plus, Precedence.Sum },
            { TokenTag.Minus, Precedence.Sum },
            { TokenTag.Asterisk, Precedence.Product },
            { TokenTag.Slash, Precedence.Product },
            { TokenTag.LParen, Precedence.Call },
            { TokenTag.LBracket, Precedence.Index },
        };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Parser(Lexer lexer)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            this.lexer = lexer;

            // read two tokens, so curToken and peekToken are both set
            NextToken();
            NextToken();

            Errors = new List<string>();
        }

        public IList<string> Errors { get; }

        public Program ParseProgram()
        {
            IList<Statement> statements = new List<Statement>();

            while (curToken.Tag != TokenTag.Eof)
            {
                Statement? statement = ParseStatement();
                if (statement is not null)
                {
                    statements.Add(statement);
                }
                NextToken();
            }

            return new Program(statements);
        }

        private Statement? ParseStatement() => curToken.Tag switch
        {
            TokenTag.Let => ParseLetStatement(),
            TokenTag.Return => ParseReturnStatement(),
            _ => ParseExpressionStatement(),
        };

        private ExpressionStatement ParseExpressionStatement()
        {
            Token token = curToken;
            var expression = ParseExpression(Precedence.Lowest);

            // we want expression statements to have optional semicolons
            // since this makes it easier to type an expression in a 
            // future REPL
            if (PeekTokenIs(TokenTag.Semicolon))
            {
                NextToken();
            }
            return new ExpressionStatement(token, expression);
        }

        private Expression? ParseExpression(Precedence precedence)
        {
            if (!PrefixParseFns.TryGetValue(curToken.Tag, out PrefixParseFn? prefix))
            {
                Errors.Add($"no prefix parse function for {curToken.Tag} found");
                return null;
            }

            var lhs = prefix(this);

            while (lhs is not null && !PeekTokenIs(TokenTag.Semicolon) && precedence < PeekPrecedence())
            {
                if (!InfixParseFns.TryGetValue(peekToken.Tag, out InfixParseFn? infix))
                {
                    return lhs;
                }
                NextToken();
                lhs = infix(this, lhs);
            }

            return lhs;
        }

        private LetStatement? ParseLetStatement()
        {
            Token letToken = curToken;

            if (!ExpectPeek(TokenTag.Ident))
            {
                return null;
            }

            var id = new Identifier(curToken);

            if (!ExpectPeek(TokenTag.Assign))
            {
                return null;
            }

            NextToken();

            var expr = ParseExpression(Precedence.Lowest);
            if (expr is null)
                return null;

            if (PeekTokenIs(TokenTag.Semicolon))
                NextToken();

            return new LetStatement(letToken, id, expr);
        }

        private ReturnStatement? ParseReturnStatement()
        {
            Token returnToken = curToken;

            NextToken(); // expression start

            var expr = ParseExpression(Precedence.Lowest);
            if (expr is null)
                return null;

            if (PeekTokenIs(TokenTag.Semicolon))
                NextToken();

            return new ReturnStatement(returnToken, expr);
        }

        private Expression ParseIdentifier()
        {
            return new Identifier(curToken);
        }

        private Expression ParseStringLiteral()
        {
            return new ork.ast.StringLiteral(curToken);
        }

        private Expression? ParseHashLiteral()
        {
            Token hashTok = curToken;
            Dictionary<Expression, Expression?> pairs = new();

            while (!PeekTokenIs(TokenTag.RBrace))
            {
                NextToken();
                var key = ParseExpression(Precedence.Lowest);
                if (key is null)
                    throw new OrkRuntimeException($"A key may not be NULL");

                if (!ExpectPeek(TokenTag.Colon))
                {
                    return null;
                }

                NextToken();
                var value = ParseExpression(Precedence.Lowest);
                pairs.Add(key, value);

                if (!PeekTokenIs(TokenTag.RBrace) && !ExpectPeek(TokenTag.Comma))
                {
                    return null;
                }
            }

            if (!ExpectPeek(TokenTag.RBrace))
                return null;
            return new HashLiteral(hashTok, pairs);
        }

        private Expression? ParseIntegerLiteral()
        {
            if (!Int64.TryParse(curToken.Literal, out Int64 val))
            {
                Errors.Add($"could not parse '{curToken.Literal}' as an integer");
                return null;
            }
            return new IntegerLiteral(curToken, val);
        }

        private Expression? ParsePrefixExpression()
        {
            Token prefixTok = curToken; // "!" or "-"
            NextToken();
            var rhs = ParseExpression(Precedence.Prefix);
            return rhs is not null ? new PrefixExpression(prefixTok, rhs) : null;
        }

        private Expression? ParseInfixExpression(Expression lhs)
        {
            Token infixTok = curToken; // "+", "-", etc
            var precedence = CurPrecedence();
            NextToken();
            var rhs = ParseExpression(precedence);
            return rhs is not null ? new InfixExpression(infixTok, lhs, rhs) : null;
        }

        private Expression ParseBooleanLiteral() => CurTokenIs(TokenTag.True) ? new TrueLiteral(curToken) : new FalseLiteral(curToken);

        private Expression? ParseGroupedExpression()
        {
            NextToken();
            var e = ParseExpression(Precedence.Lowest);
            return ExpectPeek(TokenTag.RParen) ? e : null;
        }

        private ArrayLiteral? ParseArrayLiteral()
        {
            Token arrToken = curToken;
            var elements = ParseExpressionList(TokenTag.RBracket);
            if (elements is null) return null;
            return new ArrayLiteral(arrToken, elements);
        }

        private IfExpression? ParseIfExpression()
        {
            Token ifToken = curToken;

            if (!ExpectPeek(TokenTag.LParen))
                return null;

            NextToken();
            var conditionExpression = ParseExpression(Precedence.Lowest);
            if (conditionExpression is null)
                return null;

            if (!ExpectPeek(TokenTag.RParen))
                return null;

            if (!ExpectPeek(TokenTag.LBrace))
                return null;

            var thenBlock = ParseBlockStatement();

            BlockStatement? elseBlock = null;
            if (PeekTokenIs(TokenTag.Else))
            {
                NextToken();
                if (!ExpectPeek(TokenTag.LBrace))
                    return null;
                elseBlock = ParseBlockStatement();
            }

            return new IfExpression(ifToken, conditionExpression, thenBlock, elseBlock);
        }

        private BlockStatement ParseBlockStatement()
        {
            Token braceToken = curToken; // '{'

            NextToken();

            var statements = new List<Statement>();
            while (!CurTokenIs(TokenTag.RBrace) && !CurTokenIs(TokenTag.Eof))
            {
                var stmt = ParseStatement();
                if (stmt is not null)
                    statements.Add(stmt);
                NextToken();
            }

            return new BlockStatement(braceToken, statements);
        }

        private FunctionLiteral? ParseFunctionLiteral()
        {
            Token fnToken = curToken;

            if (!ExpectPeek(TokenTag.LParen))
                return null;

            var parameters = ParseFunctionParameters();
            if (parameters is null)
                return null;

            if (!ExpectPeek(TokenTag.LBrace))
                return null;

            var body = ParseBlockStatement();
            return new FunctionLiteral(fnToken, parameters, body);
        }

        private IList<Identifier>? ParseFunctionParameters()
        {
            List<Identifier> parameters = new();

            if (PeekTokenIs(TokenTag.RParen))
            {
                NextToken();
                return parameters;
            }

            if (!ExpectPeek(TokenTag.Ident))
                return null;
            parameters.Add(new Identifier(curToken));

            while (PeekTokenIs(TokenTag.Comma))
            {
                NextToken();
                if (!ExpectPeek(TokenTag.Ident))
                    return null;
                parameters.Add(new Identifier(curToken));
            }

            if (!ExpectPeek(TokenTag.RParen))
                return null;
            
            return parameters;
        }

        private CallExpression? ParseCallExpression(Expression lhs)
        {
            Token callToken = curToken;
            var arguments = ParseExpressionList(TokenTag.RParen);
            if (arguments is null) return null;
            return new CallExpression(callToken, lhs, arguments);
        }

        private IndexExpression? ParseIndexExpression(Expression lhs)
        {
            Token indexToken = curToken;

            NextToken();
            var index = ParseExpression(Precedence.Lowest);
            if (index is null || !ExpectPeek(TokenTag.RBracket)) 
                return null;
            return new IndexExpression(indexToken, lhs, index);
        }

        private IList<Expression>? ParseExpressionList(TokenTag endTag)
        {
            List<Expression> arguments = new();

            if (PeekTokenIs(endTag))
            {
                NextToken();
                return arguments;
            }

            NextToken();
            Expression? arg = ParseExpression(Precedence.Lowest);
            if (arg is null)
                return null;
            arguments.Add(arg);

            while (PeekTokenIs(TokenTag.Comma))
            {
                NextToken();
                NextToken();
                arg = ParseExpression(Precedence.Lowest);
                if (arg is null) 
                    return null;
                arguments.Add(arg);
            }

            if (!ExpectPeek(endTag))
                return null;

            return arguments;
        }

        private Precedence CurPrecedence() => Precedences.TryGetValue(curToken.Tag, out Precedence val) ? val : Precedence.Lowest;
        private Precedence PeekPrecedence() => Precedences.TryGetValue(peekToken.Tag, out Precedence val) ? val : Precedence.Lowest;

        private bool ExpectPeek(TokenTag tag)
        {
            if (PeekTokenIs(tag))
            {
                NextToken();
                return true;
            } else
            {
                PeekError(tag);
                return false;
            }
        }

        private void PeekError(TokenTag tag)
        {
            Errors.Add($"expected next token to be '{tag.Name()}' but got '{peekToken.Tag.Name()}' instead");
        }

        private bool CurTokenIs(TokenTag tag) => curToken.Tag == tag;

        private bool PeekTokenIs(TokenTag tag) => tag == peekToken.Tag;

        private void NextToken()
        {
            curToken = peekToken;
            peekToken = lexer.NextToken();
        }
    }
}

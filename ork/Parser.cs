using ork.ast;
using ork.lexer;
using ork.tokens;

namespace ork.parser
{
    using PrefixParseFn = Func<Parser, IExpression?>;
    using InfixParseFn = Func<Parser, IExpression, IExpression?>;

    public enum Precedence
    {
        Lowest,
        Equals,
        LessGreater,
        Sum,
        Product,
        Prefix,
        Call, 
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
            { TokenTag.If, p => p.ParseIfExpression() },
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
            IList<IStatement> statements = new List<IStatement>();

            while (curToken.Tag != TokenTag.Eof)
            {
                IStatement? statement = ParseStatement();
                if (statement is not null)
                {
                    statements.Add(statement);
                }
                NextToken();
            }

            return new Program(statements);
        }

        private IStatement? ParseStatement() => curToken.Tag switch
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

        private IExpression? ParseExpression(Precedence precedence)
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

            // TODO: consume all tokens until a semicolon is
            // found
            while (!CurTokenIs(TokenTag.Semicolon))
            {
                NextToken();
            }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return new LetStatement(letToken, id, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        private ReturnStatement ParseReturnStatement()
        {
            Token returnToken = curToken;

            NextToken(); // expression start

            // TODO: consume all tokens until a semicolon is
            // found
            while (!CurTokenIs(TokenTag.Semicolon))
            {
                NextToken();
            }

            return new ReturnStatement(returnToken, null);
        }

        private IExpression ParseIdentifier()
        {
            return new Identifier(curToken);
        }

        private IExpression? ParseIntegerLiteral()
        {
            if (!Int64.TryParse(curToken.Literal, out Int64 val))
            {
                Errors.Add($"could not parse '{curToken.Literal}' as an integer");
                return null;
            }
            return new IntegerLiteral(curToken, val);
        }

        private IExpression? ParsePrefixExpression()
        {
            Token prefixTok = curToken; // "!" or "-"
            NextToken();
            var rhs = ParseExpression(Precedence.Prefix);
            return rhs is not null ? new PrefixExpression(prefixTok, rhs) : null;
        }

        private IExpression? ParseInfixExpression(IExpression lhs)
        {
            Token infixTok = curToken; // "+", "-", etc
            var precedence = CurPrecedence();
            NextToken();
            var rhs = ParseExpression(precedence);
            return rhs is not null ? new InfixExpression(infixTok, lhs, rhs) : null;
        }

        private IExpression ParseBooleanLiteral() => CurTokenIs(TokenTag.True) ? new TrueLiteral(curToken) : new FalseLiteral(curToken);

        private IExpression? ParseGroupedExpression()
        {
            NextToken();
            var e = ParseExpression(Precedence.Lowest);
            return ExpectPeek(TokenTag.RParen) ? e : null;
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

            var statements = new List<IStatement>();
            while (!CurTokenIs(TokenTag.RBrace) && !CurTokenIs(TokenTag.Eof))
            {
                var stmt = ParseStatement();
                if (stmt is not null)
                    statements.Add(stmt);
                NextToken();
            }

            return new BlockStatement(braceToken, statements);
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

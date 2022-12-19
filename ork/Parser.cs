using ork.ast;
using ork.lexer;
using ork.tokens;

namespace ork.parser
{
    public sealed class Parser
    {
        private Lexer lexer;
        private Token curToken;
        private Token peekToken;

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

        public IList<string> Errors { get; private set; }

        public Program ParseProgram()
        {
            IList<IStatement> statements = new List<IStatement>();

            while (curToken.Tag != TokenTag.Eof)
            {
                IStatement? statement = ParseStatement();
                if (statement != null)
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
            _ => null,
        };

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

            return new LetStatement(letToken, id);
        }

        private ReturnStatement? ParseReturnStatement()
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

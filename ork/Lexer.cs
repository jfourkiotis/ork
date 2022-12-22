namespace ork.lexer
{
    using tokens;

    public class Lexer
    {
        private static readonly IDictionary<string, TokenTag> Keywords =
            new Dictionary<string, TokenTag>()
            {
                { "fn", TokenTag.Function },
                { "let", TokenTag.Let },
                { "true", TokenTag.True },
                { "false", TokenTag.False },
                { "if", TokenTag.If },
                { "else", TokenTag.Else },
                { "return", TokenTag.Return },
            };

        private readonly string input;
        private int position; // current position in input (points to current char)
        private int readPosition; // current reading position in input (after current char)
        private char ch; // current char under examination

        public Lexer(string input)
        {
            this.input = input;
            ReadChar(); // initialize lexer state
        }
        public Token NextToken()
        {
            SkipWhitespace();

            bool readNext = true;
            Token tok = ch switch
            {
                '=' => ReadAssignmentOrEq(),
                ';' => new Token(TokenTag.Semicolon, ch.ToString()),
                '(' => new Token(TokenTag.LParen, ch.ToString()),
                ')' => new Token(TokenTag.RParen, ch.ToString()),
                '{' => new Token(TokenTag.LBrace, ch.ToString()),
                '}' => new Token(TokenTag.RBrace, ch.ToString()),
                ',' => new Token(TokenTag.Comma, ch.ToString()),
                '+' => new Token(TokenTag.Plus, ch.ToString()),
                '-' => new Token(TokenTag.Minus, ch.ToString()),
                '*' => new Token(TokenTag.Asterisk, ch.ToString()),
                '/' => new Token(TokenTag.Slash, ch.ToString()),
                '!' => ReadBangOrNotEq(),
                '<' => new Token(TokenTag.LessThan, ch.ToString()),
                '>' => new Token(TokenTag.GreaterThan, ch.ToString()),
                '\0' => new Token(TokenTag.Eof, ""),
                var _ when Char.IsLetter(ch) => ReadIdentifierOrKeyword(out readNext), 
                var _ when Char.IsDigit(ch) => ReadNumber(out readNext),
                _ => throw new NotImplementedException($"{ch}"),
            };

            if (readNext)
            {
                ReadChar();
            }
            return tok;
        }

        private void SkipWhitespace()
        {
            while (Char.IsWhiteSpace(ch))
            {
                ReadChar();
            }
        }

        private void ReadChar()
        {
            if (readPosition >= input.Length)
            {
                ch = Char.MinValue; // '\0'
            } else
            {
                ch = input[readPosition];
            }
            position = readPosition;
            readPosition++;
        }

        private char PeekNext()
        {
            if (readPosition >= input.Length)
            {
                return '\0';
            }
            return input[readPosition];
        }

        private Token ReadIdentifierOrKeyword(out bool readNext)
        {
            readNext = false;
            int pos = position;
            // I know the first character is a letter
            while (Char.IsLetterOrDigit(ch) || ch == '_')
            {
                ReadChar();
            }

            var literal = input.Substring(pos, position - pos);

            if (Keywords.TryGetValue(literal, out TokenTag keyTag))
            {
                return new Token(keyTag, literal);
            }
            return new Token(TokenTag.Ident, literal);
        }

        private Token ReadNumber(out bool readNext)
        {
            readNext = false;
            int pos = position;
            while (Char.IsDigit(ch))
            {
                ReadChar();
            }

            var number = input.Substring(pos, position - pos);
            return new Token(TokenTag.Int, number);
        }

        private Token ReadAssignmentOrEq()
        {
            if (PeekNext() == '=')
            {
                ReadChar();
                return new Token(TokenTag.Eq, "==");
            }
            return new Token(TokenTag.Assign, "=");
        }
        private Token ReadBangOrNotEq()
        {
            if (PeekNext() == '=')
            {
                ReadChar();
                return new Token(TokenTag.NotEq, "!=");
            }
            return new Token(TokenTag.Bang, "!");
        }
    }
}

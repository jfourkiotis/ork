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
        private int line = 1; // current line
        private int line_offset = 0;

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
                ';' => new Token(TokenTag.Semicolon, ch.ToString(), line, position+1-line_offset),
                ':' => new Token(TokenTag.Colon, ch.ToString(), line, position+1-line_offset),
                '(' => new Token(TokenTag.LParen, ch.ToString(), line, position+1-line_offset),
                ')' => new Token(TokenTag.RParen, ch.ToString(), line, position+1-line_offset),
                '{' => new Token(TokenTag.LBrace, ch.ToString(), line, position+1-line_offset),
                '}' => new Token(TokenTag.RBrace, ch.ToString(), line, position+1-line_offset),
                '[' => new Token(TokenTag.LBracket, ch.ToString(), line, position+1-line_offset),
                ']' => new Token(TokenTag.RBracket, ch.ToString(), line, position+1-line_offset),
                ',' => new Token(TokenTag.Comma, ch.ToString(), line, position+1-line_offset),
                '+' => new Token(TokenTag.Plus, ch.ToString(), line, position+1-line_offset),
                '-' => new Token(TokenTag.Minus, ch.ToString(), line, position+1-line_offset),
                '*' => new Token(TokenTag.Asterisk, ch.ToString(), line, position+1-line_offset),
                '/' => new Token(TokenTag.Slash, ch.ToString(), line, position+1-line_offset),
                '!' => ReadBangOrNotEq(),
                '<' => new Token(TokenTag.LessThan, ch.ToString(), line, position+1-line_offset),
                '>' => new Token(TokenTag.GreaterThan, ch.ToString(), line, position+1-line_offset),
                '#' => ConsumeCommentAndContinue(out readNext),
                '"' => ReadString(),
                '\0' => new Token(TokenTag.Eof, "", line, position+1-line_offset),
                _ when Char.IsLetter(ch) => ReadIdentifierOrKeyword(out readNext), 
                _ when Char.IsDigit(ch) => ReadNumber(out readNext),
                _ => throw new OrkRuntimeException($"Invalid character: {ch}"),
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
                if (ch == '\n')
                {
                    line++;
                    // one character past the '\n' character
                    line_offset = readPosition;
                }
                ReadChar();
            }
        }

        private void ReadChar()
        {
            ch = readPosition >= input.Length ? Char.MinValue : // '\0'
                input[readPosition];
            position = readPosition;
            readPosition++;
        }

        private char PeekNext() => readPosition >= input.Length ? Char.MinValue : input[readPosition];

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
            return Keywords.TryGetValue(literal, out TokenTag keyTag) ? new Token(keyTag, literal, line, pos - line_offset + 1) : new Token(TokenTag.Ident, literal, line, pos - line_offset + 1);
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
            return new Token(TokenTag.Int, number, line, pos - line_offset + 1);
        }

        private Token ReadAssignmentOrEq()
        {
            if (PeekNext() != '=') return new Token(TokenTag.Assign, "=", line, position+1-line_offset);
            ReadChar();
            return new Token(TokenTag.Eq, "==", line, position - line_offset);
        }
        private Token ReadBangOrNotEq()
        {
            if (PeekNext() != '=') return new Token(TokenTag.Bang, "!", line, position+1-line_offset);
            ReadChar();
            return new Token(TokenTag.NotEq, "!=", line, position - line_offset);
        }

        private Token ConsumeCommentAndContinue(out bool readNext)
        {
            readNext = false;
            while (ch != '\n' && ch != Char.MinValue)
                ReadChar();
            line++;
            return NextToken();
        }

        private Token ReadString()
        {
            var pos = position + 1;
            while (true)
            {
                ReadChar();
                if (ch == '"' || ch == Char.MinValue)
                    break;
            }

            return new Token(TokenTag.String, input.Substring(pos, position - pos), line, pos - line_offset);
        }
    }
}

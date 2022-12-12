namespace ork.tokens
{
    public enum TokenTag
    {
        Illegal,
        Eof,
        // identifiers + literals
        Ident,
        Int,
        // operators
        Assign,
        Plus,
        Minus,
        Bang,
        Asterisk,
        Slash,
        LessThan,
        GreaterThan,
        Eq,
        NotEq,
        // delimiters
        Comma,
        Semicolon,

        LParen,
        RParen,
        LBrace,
        RBrace,

        // keywords
        Function,
        Let,
        True,
        False,
        Else,
        Return,
        If,
    }

    public static class TokenTagExtensions
    {
        public static string Name(this TokenTag tag) => tag switch
        {
            TokenTag.Illegal => "illegal",
            TokenTag.Eof => "eof",
            TokenTag.Ident => "ident",
            TokenTag.Int => "int",
            TokenTag.Assign => "=",
            TokenTag.Plus => "+",
            TokenTag.Comma => ",",
            TokenTag.Semicolon => ";",
            TokenTag.LParen => "(",
            TokenTag.RParen => ")",
            TokenTag.LBrace => "{",
            TokenTag.RBrace => "}",
            TokenTag.Function => "fn",
            TokenTag.Let => "let",
            TokenTag.Minus => "-",
            TokenTag.Bang => "!",
            TokenTag.Asterisk => "*",
            TokenTag.Slash => "/",
            TokenTag.LessThan => "<",
            TokenTag.GreaterThan => ">",
            TokenTag.True => "true",
            TokenTag.False => "false",
            TokenTag.Else => "else",
            TokenTag.Return => "return",
            TokenTag.If => "if",
            TokenTag.Eq => "==",
            TokenTag.NotEq => "!=",
            _ => throw new NotImplementedException(),
        };
    }
}

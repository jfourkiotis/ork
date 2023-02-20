namespace ork.tokens
{
    public record Token(TokenTag Tag, string Literal, int Line, int Pos);
}

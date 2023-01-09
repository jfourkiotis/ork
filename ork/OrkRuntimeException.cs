namespace ork;

public class OrkRuntimeException : Exception
{
    public OrkRuntimeException()
    {
    }

    public OrkRuntimeException(string message) : base(message)
    {
    }

    public OrkRuntimeException(string message, Exception inner) : base(message, inner)
    {
    }
}
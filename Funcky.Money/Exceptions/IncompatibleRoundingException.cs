namespace Funcky;

[Serializable]
public sealed class IncompatibleRoundingException : Exception
{
    public IncompatibleRoundingException()
    {
    }

    public IncompatibleRoundingException(string? message)
        : base(message)
    {
    }

    public IncompatibleRoundingException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

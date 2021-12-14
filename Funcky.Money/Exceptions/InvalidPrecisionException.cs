using System.Runtime.Serialization;

namespace Funcky;

[Serializable]
public class InvalidPrecisionException : Exception
{
    public InvalidPrecisionException()
        : base("precision must be positive and cannot be zero")
    {
    }

    public InvalidPrecisionException(string? message)
        : base(message)
    {
    }

    public InvalidPrecisionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected InvalidPrecisionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

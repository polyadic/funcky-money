#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Funcky;

public class MissingExchangeRateException : Exception
{
    public MissingExchangeRateException()
        : base("If you calculate with more than one currency, you have to define the exchange rate and target in the evaluation context.")
    {
    }

    public MissingExchangeRateException(string? message)
        : base(message)
    {
    }

    public MissingExchangeRateException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

#if NETSTANDARD2_0
    protected MissingExchangeRateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}

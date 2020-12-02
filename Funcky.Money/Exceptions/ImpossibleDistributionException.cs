using System;
using System.Runtime.Serialization;

namespace Funcky
{
    [Serializable]
    internal class ImpossibleDistributionException : Exception
    {
        public ImpossibleDistributionException()
        {
        }

        public ImpossibleDistributionException(string? message)
            : base(message)
        {
        }

        public ImpossibleDistributionException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected ImpossibleDistributionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

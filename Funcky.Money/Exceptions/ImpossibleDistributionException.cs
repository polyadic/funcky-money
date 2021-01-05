using System;

namespace Funcky
{
    [Serializable]
    public sealed class ImpossibleDistributionException : Exception
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
    }
}

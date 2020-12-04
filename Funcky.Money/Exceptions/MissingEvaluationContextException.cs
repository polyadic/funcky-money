using System;
using System.Runtime.Serialization;

namespace Funcky
{
    [Serializable]
    public sealed class MissingEvaluationContextException : Exception
    {
        public MissingEvaluationContextException()
        {
        }

        public MissingEvaluationContextException(string? message)
            : base(message)
        {
        }

        public MissingEvaluationContextException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

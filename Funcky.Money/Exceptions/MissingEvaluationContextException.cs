using System;
using System.Runtime.Serialization;

namespace Funcky
{
    [Serializable]
    public class MissingEvaluationContextException : Exception
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

        protected MissingEvaluationContextException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

using System;

namespace Funcky
{
    [Serializable]
    internal sealed class InvalidMoneyEvaluationContextBuilderException : Exception
    {
        public InvalidMoneyEvaluationContextBuilderException()
        {
        }

        public InvalidMoneyEvaluationContextBuilderException(string? message)
            : base(message)
        {
        }

        public InvalidMoneyEvaluationContextBuilderException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

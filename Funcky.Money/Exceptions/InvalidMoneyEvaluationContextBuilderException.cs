namespace Funcky;

[Serializable]
public sealed class InvalidMoneyEvaluationContextBuilderException : Exception
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

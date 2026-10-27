namespace Funcky;

public sealed class MissingEvaluationContextException()
    : Exception("Evaluating an expression with more than one currency requires a MoneyEvaluationContext with exchange rates.");

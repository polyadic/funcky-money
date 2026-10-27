namespace Funcky;

public interface IMoneyExpression
{
    TResult Accept<TResult>(IMoneyExpressionVisitor<TResult> visitor);
}

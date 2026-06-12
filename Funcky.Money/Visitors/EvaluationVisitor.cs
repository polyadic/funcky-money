using Funcky.Monads;

namespace Funcky;

internal sealed class EvaluationVisitor(
    IDistributionStrategy distributionStrategy,
    Option<MoneyEvaluationContext> context)
    : IMoneyExpressionVisitor<MoneyBag>
{
    public MoneyBag Visit(Money money)
       => new(money);

    public MoneyBag Visit(MoneySum sum)
        => Accept(sum.Augend)
            .Merge(Accept(sum.Addend));

    public MoneyBag Visit(MoneyDifference difference)
        => Accept(difference.Minuend)
            .Subtract(Accept(difference.Subtrahend));

    public MoneyBag Visit(MoneyProduct product)
        => Accept(product.Expression)
            .Multiply(product.Factor);

    public MoneyBag Visit(MoneyQuotient quotient)
        => Accept(quotient.Expression)
            .Divide(quotient.Divisor);

    public MoneyBag Visit(MoneyDistributionPart part)
        => new(distributionStrategy.Distribute(part, CalculateTotal(part)));

    private MoneyBag Accept(IMoneyExpression expression)
        => expression
            .Accept(this);

    private Money CalculateTotal(MoneyDistributionPart part)
        => Accept(part.Distribution.Expression)
            .CalculateTotal(context);
}

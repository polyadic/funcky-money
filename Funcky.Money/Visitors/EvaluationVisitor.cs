using System.Numerics;
using Funcky.Monads;

namespace Funcky;

internal sealed class EvaluationVisitor<TUnderlyingType> : IMoneyExpressionVisitor<TUnderlyingType, MoneyBag<TUnderlyingType>>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    private readonly IDistributionStrategy<TUnderlyingType> _distributionStrategy;
    private readonly Option<MoneyEvaluationContext<TUnderlyingType>> _context;

    public EvaluationVisitor(IDistributionStrategy<TUnderlyingType> distributionStrategy, Option<MoneyEvaluationContext<TUnderlyingType>> context)
    {
        _distributionStrategy = distributionStrategy;
        _context = context;
    }

    public MoneyBag<TUnderlyingType> Visit(Money<TUnderlyingType> money)
       => new(money);

    public MoneyBag<TUnderlyingType> Visit(MoneySum<TUnderlyingType> sum)
        => Accept(sum.Left)
            .Merge(Accept(sum.Right));

    public MoneyBag<TUnderlyingType> Visit(MoneyProduct<TUnderlyingType> product)
        => Accept(product.Expression)
            .Multiply(product.Factor);

    public MoneyBag<TUnderlyingType> Visit(MoneyDistributionPart<TUnderlyingType> part)
        => new(_distributionStrategy.Distribute(part, CalculateTotal(part)));

    private MoneyBag<TUnderlyingType> Accept(IMoneyExpression<TUnderlyingType> expression)
        => expression
            .Accept(this);

    private Money<TUnderlyingType> CalculateTotal(MoneyDistributionPart<TUnderlyingType> part)
        => Accept(part.Distribution.Expression)
            .CalculateTotal(_context);
}

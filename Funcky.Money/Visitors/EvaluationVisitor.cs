using Funcky.Monads;

namespace Funcky
{
    internal sealed class EvaluationVisitor : IMoneyExpressionVisitor<MoneyBag>
    {
        private readonly IDistributionStrategy _distributionStrategy;
        private readonly Option<MoneyEvaluationContext> _context;

        public EvaluationVisitor(IDistributionStrategy distributionStrategy, Option<MoneyEvaluationContext> context)
            => (_distributionStrategy, _context) = (distributionStrategy, context);

        public MoneyBag Visit(Money money)
           => new(money);

        public MoneyBag Visit(MoneySum sum)
            => Accept(sum.Left)
                .Merge(Accept(sum.Right));

        public MoneyBag Visit(MoneyProduct product)
            => Accept(product.Expression)
                .Multiply(product.Factor);

        public MoneyBag Visit(MoneyDistributionPart part)
            => new(_distributionStrategy.Distribute(part, CalculateTotal(part)));

        private MoneyBag Accept(IMoneyExpression expression)
            => expression
                .Accept(this);

        private Money CalculateTotal(MoneyDistributionPart part)
            => Accept(part.Distribution.Expression)
                .CalculateTotal(_context);
    }
}

using System.Collections.Generic;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly IDistributionStrategy _distributionStrategy;
        private readonly Option<MoneyEvaluationContext> _context;

        private readonly Stack<MoneyBag> _moneyBags = new();

        public EvaluationVisitor(IDistributionStrategy distributionStrategy, Option<MoneyEvaluationContext> context)
        {
            _distributionStrategy = distributionStrategy;
            _context = context;
            PushMoneyBag();
        }

        public Money Result
            => Round(_moneyBags.Peek().CalculateTotal(_context));

        public void Visit(Money money)
            => _moneyBags.Peek().Add(money);

        public void Visit(MoneySum sum)
        {
            sum.Left.Accept(this);

            PushMoneyBag();
            sum.Right.Accept(this);

            var moneyBags = _moneyBags.Pop();
            _moneyBags.Peek().Merge(moneyBags);
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);

            _moneyBags.Peek().Multiply(product.Factor);
        }

        public void Visit(MoneyDistributionPart part)
        {
            part.Distribution.Expression.Accept(this);

            PushMoneyBag(_distributionStrategy.Distribute(part, _moneyBags.Pop().CalculateTotal(_context)));
        }

        private void PushMoneyBag(Option<Money> money = default)
        {
            var moneyBag = new MoneyBag();

            money.AndThen(m => moneyBag.Add(m));

            _moneyBags.Push(moneyBag);
        }

        private Money Round(Money money)
            => money with { Amount = FindRoundingStrategy(money).Round(money.Amount) };

        private IRoundingStrategy FindRoundingStrategy(Money money)
            => _context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
    }
}

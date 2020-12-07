using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly Option<MoneyEvaluationContext> _context;

        private Stack<MoneyBag> _moneyBags = new();

        public EvaluationVisitor(Option<MoneyEvaluationContext> context)
        {
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
            ((IMoneyExpression)part.Distribution).Accept(this);

            var money = _moneyBags.Pop().CalculateTotal(_context);
            var partAmount = SliceAmount(part, money);

            if (RoundingStrategy(money) is not NoRounding && decimal.Remainder(ToDistribute(part, money), Precision(money)) != 0m)
            {
                throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part, money)} in sizes of {Precision(money)}");
            }

            PushMoneyBag(money with { Amount = partAmount, Currency = money.Currency });
        }

        public void Visit(MoneyDistribution distribution)
            => distribution.Expression.Accept(this);

        private decimal SliceAmount(MoneyDistributionPart part, Money money)
            => part.Index switch
            {
                _ when Precision(money) * (part.Index + 1) < ToDistribute(part, money) => Slice(part.Distribution, part.Index, money) + Precision(money),
                _ when Precision(money) * part.Index < ToDistribute(part, money) => Slice(part.Distribution, part.Index, money) + ToDistribute(part, money) - AlreadyDistributed(part, money),
                _ => Slice(part.Distribution, part.Index, money),
            };

        private void PushMoneyBag(Option<Money> money = default)
        {
            var moneyBag = new MoneyBag();

            money.AndThen(m => moneyBag.Add(m));

            _moneyBags.Push(moneyBag);
        }

        private decimal AlreadyDistributed(MoneyDistributionPart part, Money money)
            => Precision(money) * part.Index;

        private decimal Precision(Money money)
            => RoundingStrategy(money).Precision;

        private AbstractRoundingStrategy RoundingStrategy(Money money)
            => _context.Match(some: c => c.RoundingStrategy, none: money.RoundingStrategy);

        private decimal ToDistribute(MoneyDistributionPart part, Money money)
            => money.Amount - DistributedTotal(part, money);

        private decimal DistributedTotal(MoneyDistributionPart part, Money money)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index, money));

        private decimal Slice(MoneyDistribution distribution, int index, Money money)
            => Truncate(ExactSlice(distribution, index, money), Precision(money));

        private decimal ExactSlice(MoneyDistribution distribution, int index, Money money)
            => money.Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();

        private Money Round(Money money)
            => money with { Amount = FindRoundingStrategy(money).Round(money.Amount) };

        private AbstractRoundingStrategy FindRoundingStrategy(Money money)
            => _context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
    }
}

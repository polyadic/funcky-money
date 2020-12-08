using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly Option<MoneyEvaluationContext> _context;

        private readonly Stack<MoneyBag> _moneyBags = new();

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
            part.Distribution.Expression.Accept(this);

            var total = _moneyBags.Pop().CalculateTotal(_context);

            if (IsDistributable(part, total))
            {
                PushMoneyBag(total with { Amount = SliceAmount(part, total), Currency = total.Currency });
            }
            else
            {
                throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part, total)} in sizes of {Precision(part.Distribution, total)} with the current Rounding strategy: {RoundingStrategy(total)}.");
            }
        }

        private bool IsDistributable(MoneyDistributionPart part, Money money)
            => RoundingStrategy(money).IsSameAfterRounding(Precision(part.Distribution, money))
               && RoundingStrategy(money).IsSameAfterRounding(ToDistribute(part, money));

        private decimal SliceAmount(MoneyDistributionPart part, Money money)
            => part.Index switch
            {
                _ when Precision(part.Distribution, money) * (part.Index + 1) < ToDistribute(part, money) => Slice(part.Distribution, part.Index, money) + Precision(part.Distribution, money),
                _ when Precision(part.Distribution, money) * part.Index < ToDistribute(part, money) => Slice(part.Distribution, part.Index, money) + ToDistribute(part, money) - AlreadyDistributed(part, money),
                _ => Slice(part.Distribution, part.Index, money),
            };

        private void PushMoneyBag(Option<Money> money = default)
        {
            var moneyBag = new MoneyBag();

            money.AndThen(m => moneyBag.Add(m));

            _moneyBags.Push(moneyBag);
        }

        private decimal AlreadyDistributed(MoneyDistributionPart part, Money money)
            => Precision(part.Distribution, money) * part.Index;

        // Order of evaluation: Distribution > Context Distribution > Context Currency > Money Currency
        private decimal Precision(MoneyDistribution distribution, Money money) =>
            distribution
                .Precision
                .OrElse(_context.AndThen(c => c.DistributionUnit))
                .GetOrElse(Power.OfATenth(MinorUnitDigits(money)));

        private int MinorUnitDigits(Money money) =>
            _context.Match(
                none: money.Currency.MinorUnitDigits,
                some: c => c.TargetCurrency.MinorUnitDigits);

        private IRoundingStrategy RoundingStrategy(Money money)
            => _context.Match(
                some: c => c.RoundingStrategy,
                none: money.RoundingStrategy);

        private decimal ToDistribute(MoneyDistributionPart part, Money money)
            => money.Amount - DistributedTotal(part, money);

        private decimal DistributedTotal(MoneyDistributionPart part, Money money)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index, money));

        private decimal Slice(MoneyDistribution distribution, int index, Money money)
            => Truncate(ExactSlice(distribution, index, money), Precision(distribution, money));

        private static decimal ExactSlice(MoneyDistribution distribution, int index, Money money)
            => money.Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();

        private Money Round(Money money)
            => money with { Amount = FindRoundingStrategy(money).Round(money.Amount) };

        private IRoundingStrategy FindRoundingStrategy(Money money)
            => _context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
    }
}

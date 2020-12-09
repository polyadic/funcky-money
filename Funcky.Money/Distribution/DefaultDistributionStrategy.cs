using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class DefaultDistributionStrategy : IDistributionStrategy
    {
        private readonly Option<MoneyEvaluationContext> _context;

        public DefaultDistributionStrategy(Option<MoneyEvaluationContext> context)
        {
            _context = context;
        }

        public Money Distribute(MoneyDistributionPart part, Money total)
            => IsDistributable(part, total)
                ? total with { Amount = SliceAmount(part, total), Currency = total.Currency }
                : throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part, total)} in sizes of {Precision(part.Distribution, total)} with the current Rounding strategy: {RoundingStrategy(total)}.");

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

        private decimal AlreadyDistributed(MoneyDistributionPart part, Money money)
            => Precision(part.Distribution, money) * part.Index;

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

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();
    }
}

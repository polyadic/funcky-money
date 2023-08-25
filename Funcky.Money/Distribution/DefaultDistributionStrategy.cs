using System.Numerics;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal class DefaultDistributionStrategy<TUnderlyingType> : IDistributionStrategy<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    private readonly Option<MoneyEvaluationContext<TUnderlyingType>> _context;

    public DefaultDistributionStrategy(Option<MoneyEvaluationContext<TUnderlyingType>> context)
    {
        _context = context;
    }

    public Money<TUnderlyingType> Distribute(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> total)
        => IsDistributable(part, total)
            ? total with { Amount = SliceAmount(part, total), Currency = total.Currency }
            : throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part, total)} in sizes of {Precision(part.Distribution, total)} with the current Rounding strategy: {RoundingStrategy(total)}.");

    private bool IsDistributable(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => RoundingStrategy(money).IsSameAfterRounding(Precision(part.Distribution, money))
           && RoundingStrategy(money).IsSameAfterRounding(ToDistribute(part, money));

    private TUnderlyingType SliceAmount(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => Slice(part.Distribution, part.Index, money) + DistributeRest(part, money);

    private TUnderlyingType DistributeRest(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => part.Index switch
        {
            _ when AtLeastOneDistributionUnitLeft(part, money) => SignedPrecision(part.Distribution, money),
            _ when BetweenZeroToOneDistributionUnitLeft(part, money) => ToDistribute(part, money) - AlreadyDistributed(part, money),
            _ => TUnderlyingType.Zero,
        };

    private bool AtLeastOneDistributionUnitLeft(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => Precision(part.Distribution, money) * TUnderlyingType.CreateChecked(part.Index + 1) < TUnderlyingType.Abs(ToDistribute(part, money));

    private bool BetweenZeroToOneDistributionUnitLeft(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => Precision(part.Distribution, money) * TUnderlyingType.CreateChecked(part.Index) < TUnderlyingType.Abs(ToDistribute(part, money));

    private TUnderlyingType AlreadyDistributed(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => SignedPrecision(part.Distribution, money) * TUnderlyingType.CreateChecked(part.Index);

    private IRoundingStrategy<TUnderlyingType> RoundingStrategy(Money<TUnderlyingType> money)
        => _context.Match(
            some: c => c.RoundingStrategy,
            none: money.RoundingStrategy);

    private TUnderlyingType ToDistribute(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => money.Amount - DistributedTotal(part, money);

    private TUnderlyingType DistributedTotal(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> money)
        => part
            .Distribution
            .Factors
            .WithIndex()
            .Aggregate(TUnderlyingType.Zero, (sum, value) => sum + Slice(part.Distribution, value.Index, money));

    private TUnderlyingType Slice(MoneyDistribution<TUnderlyingType> distribution, int index, Money<TUnderlyingType> money)
        => Truncate(ExactSlice(distribution, index, money), Precision(distribution, money));

    private static TUnderlyingType ExactSlice(MoneyDistribution<TUnderlyingType> distribution, int index, Money<TUnderlyingType> money)
        => money.Amount / TUnderlyingType.CreateChecked(DistributionTotal(distribution)) * TUnderlyingType.CreateChecked(distribution.Factors[index]);

    private TUnderlyingType SignedPrecision(MoneyDistribution<TUnderlyingType> distribution, Money<TUnderlyingType> money)
        => TUnderlyingType.CopySign(Precision(distribution, money), money.Amount);

    // Order of evaluation: Distribution > Context Distribution > Context Currency > Money Currency
    private TUnderlyingType Precision(MoneyDistribution<TUnderlyingType> distribution, Money<TUnderlyingType> money)
        => distribution
            .Precision
            .OrElse(_context.AndThen(c => c.DistributionUnit))
            .GetOrElse(Power<TUnderlyingType>.OfATenth(MinorUnitDigits(money)));

    private int MinorUnitDigits(Money<TUnderlyingType> money)
        => _context.Match(
            none: money.Currency.MinorUnitDigits,
            some: c => c.TargetCurrency.MinorUnitDigits);

    private static TUnderlyingType Truncate(TUnderlyingType amount, TUnderlyingType precision)
        => TUnderlyingType.Truncate(amount / precision) * precision;

    private static int DistributionTotal(MoneyDistribution<TUnderlyingType> distribution)
        => distribution.Factors.Sum();
}

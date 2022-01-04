using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal class DefaultDistributionStrategy : IDistributionStrategy
{
    private readonly Option<MoneyEvaluationContext> _context;

    public DefaultDistributionStrategy(Option<MoneyEvaluationContext> context)
    {
        _context = context;
    }

    public MoneyExpression.Money Distribute(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money total)
        => IsDistributable(part, total)
            ? total with { Amount = SliceAmount(part, total), Currency = total.Currency }
            : throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part, total)} in sizes of {Precision(part.Distribution, total)} with the current Rounding strategy: {RoundingStrategy(total)}.");

    private bool IsDistributable(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => RoundingStrategy(money).IsSameAfterRounding(Precision(part.Distribution, money))
           && RoundingStrategy(money).IsSameAfterRounding(ToDistribute(part, money));

    private decimal SliceAmount(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => Slice(part.Distribution, part.Index, money) + DistributeRest(part, money);

    private decimal DistributeRest(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => part.Index switch
        {
            _ when AtLeastOneDistributionUnitLeft(part, money) => SignedPrecision(part.Distribution, money),
            _ when BetweenZeroToOneDistributionUnitLeft(part, money) => ToDistribute(part, money) - AlreadyDistributed(part, money),
            _ => 0.0m,
        };

    private bool AtLeastOneDistributionUnitLeft(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => Precision(part.Distribution, money) * (part.Index + 1) < Math.Abs(ToDistribute(part, money));

    private bool BetweenZeroToOneDistributionUnitLeft(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => Precision(part.Distribution, money) * part.Index < Math.Abs(ToDistribute(part, money));

    private decimal AlreadyDistributed(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => SignedPrecision(part.Distribution, money) * part.Index;

    private IRoundingStrategy RoundingStrategy(MoneyExpression.Money money)
        => _context.Match(
            some: c => c.RoundingStrategy,
            none: money.RoundingStrategy);

    private decimal ToDistribute(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => money.Amount - DistributedTotal(part, money);

    private decimal DistributedTotal(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money money)
        => part
            .Distribution
            .Factors
            .WithIndex()
            .Sum(f => Slice(part.Distribution, f.Index, money));

    private decimal Slice(MoneyExpression.MoneyDistribution distribution, int index, MoneyExpression.Money money)
        => Truncate(ExactSlice(distribution, index, money), Precision(distribution, money));

    private static decimal ExactSlice(MoneyExpression.MoneyDistribution distribution, int index, MoneyExpression.Money money)
        => money.Amount / DistributionTotal(distribution) * distribution.Factors[index];

    private decimal SignedPrecision(MoneyExpression.MoneyDistribution distribution, MoneyExpression.Money money)
        => Precision(distribution, money).CopySign(money.Amount);

    // Order of evaluation: Distribution > Context Distribution > Context Currency > Money Currency
    private decimal Precision(MoneyExpression.MoneyDistribution distribution, MoneyExpression.Money money)
        => distribution
            .Precision
            .OrElse(_context.AndThen(c => c.DistributionUnit))
            .GetOrElse(Power.OfATenth(MinorUnitDigits(money)));

    private int MinorUnitDigits(MoneyExpression.Money money)
        => _context.Match(
            none: money.Currency.MinorUnitDigits,
            some: c => c.TargetCurrency.MinorUnitDigits);

    private static decimal Truncate(decimal amount, decimal precision)
        => decimal.Truncate(amount / precision) * precision;

    private static int DistributionTotal(MoneyExpression.MoneyDistribution distribution)
        => distribution.Factors.Sum();
}

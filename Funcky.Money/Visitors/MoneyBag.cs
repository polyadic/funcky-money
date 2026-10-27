using System.Collections.Immutable;

namespace Funcky;

/// <summary>
/// Amounts per currency plus the rounding strategies of the moneys involved. Conversion into a single currency is
/// deferred until a result is needed, so that exchange rates are applied as late as possible.
/// </summary>
internal sealed class MoneyBag
{
    private readonly ImmutableDictionary<Currency, decimal> _amounts;
    private readonly ImmutableHashSet<IRoundingStrategy> _roundingStrategies;

    private MoneyBag(ImmutableDictionary<Currency, decimal> amounts, ImmutableHashSet<IRoundingStrategy> roundingStrategies)
        => (_amounts, _roundingStrategies) = (amounts, roundingStrategies);

    public static MoneyBag Of(Money money)
        => new(
            ImmutableDictionary<Currency, decimal>.Empty.Add(money.Currency, money.Amount),
            ImmutableHashSet.Create(money.RoundingStrategy));

    public MoneyBag Merge(MoneyBag other)
        => new(other._amounts.Aggregate(_amounts, AddAmount), _roundingStrategies.Union(other._roundingStrategies));

    public MoneyBag Multiply(decimal factor)
        => new(_amounts.ToImmutableDictionary(entry => entry.Key, entry => entry.Value * factor), _roundingStrategies);

    // A true division is more exact than multiplying with the inverse of the divisor.
    public MoneyBag Divide(decimal divisor)
        => new(_amounts.ToImmutableDictionary(entry => entry.Key, entry => entry.Value / divisor), _roundingStrategies);

    /// <summary>Converts the bag into a single, unrounded money.</summary>
    public Money ToMoney(Option<MoneyEvaluationContext> context)
        => context.Match(
            none: ToMoneyWithoutConversion,
            some: ToMoneyInTargetCurrency);

    private static ImmutableDictionary<Currency, decimal> AddAmount(ImmutableDictionary<Currency, decimal> amounts, KeyValuePair<Currency, decimal> entry)
        => amounts.SetItem(entry.Key, amounts.GetValueOrDefault(entry.Key) + entry.Value);

    // Without a context the result is only well-defined if all moneys agree on currency and rounding.
    private Money ToMoneyWithoutConversion()
        => _amounts.Count == 1 && _roundingStrategies.Count == 1
            ? new Money(_amounts.Values.Single(), _amounts.Keys.Single(), _roundingStrategies.Single())
            : throw new MissingEvaluationContextException();

    private Money ToMoneyInTargetCurrency(MoneyEvaluationContext context)
        => new(_amounts.Sum(entry => entry.Value * context.ExchangeRate(entry.Key)), context.TargetCurrency, context.RoundingStrategy);
}

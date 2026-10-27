namespace Funcky;

/// <summary>
/// Amounts per currency. Conversion into a single currency is deferred until a result is needed, so that
/// exchange rates are applied as late as possible.
/// </summary>
internal sealed class MoneyBag
{
    private readonly IReadOnlyDictionary<Currency, decimal> _amounts;

    private MoneyBag(IReadOnlyDictionary<Currency, decimal> amounts)
        => _amounts = amounts;

    public static MoneyBag Of(Money money)
        => new(new Dictionary<Currency, decimal> { [money.Currency] = money.Amount });

    public MoneyBag Merge(MoneyBag other)
        => new(_amounts
            .Concat(other._amounts)
            .GroupBy(entry => entry.Key, entry => entry.Value)
            .ToDictionary(group => group.Key, group => group.Sum()));

    public MoneyBag Multiply(decimal factor)
        => new(_amounts.ToDictionary(entry => entry.Key, entry => entry.Value * factor));

    public Money ToMoney(Option<MoneyEvaluationContext> context)
        => context.Match(
            none: ToMoneyWithoutConversion,
            some: ToMoneyInTargetCurrency);

    private Money ToMoneyWithoutConversion()
        => _amounts.Count == 1
            ? new Money(_amounts.Values.Single(), _amounts.Keys.Single())
            : throw new MissingEvaluationContextException();

    private Money ToMoneyInTargetCurrency(MoneyEvaluationContext context)
        => new(_amounts.Sum(entry => entry.Value * context.ExchangeRate(entry.Key)), context.TargetCurrency);
}

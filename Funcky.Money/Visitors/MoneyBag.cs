using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal sealed class MoneyBag
{
    private readonly Dictionary<Currency, List<Money>> _currencies = new();
    private Option<IRoundingStrategy> _roundingStrategy;

    public MoneyBag(Money money)
    {
        Add(money);
    }

    public MoneyBag Merge(MoneyBag moneyBag)
    {
        moneyBag
            ._currencies
            .SelectMany(kv => kv.Value)
            .ForEach(Add);

        return this;
    }

    public MoneyBag Multiply(decimal factor)
    {
        _currencies
            .ForEach(kv => _currencies[kv.Key] = MultiplyBag(factor, kv.Value));

        return this;
    }

    public Money CalculateTotal(Option<MoneyEvaluationContext> context)
        => context.Match(
            none: AggregateWithoutEvaluationContext,
            some: AggregateWithEvaluationContext);

    private void Add(Money money)
    {
        CreateMoneyBagByCurrency(money.Currency);
        _currencies[money.Currency].Add(money);
    }

    private static List<Money> MultiplyBag(decimal factor, IEnumerable<Money> bag)
        => bag
            .Select(m => m with { Amount = m.Amount * factor })
            .ToList();

    private Money AggregateWithEvaluationContext(MoneyEvaluationContext context)
        => _currencies
            .Values
            .Select(c => c.Aggregate(MoneySum(context)))
            .Select(ExchangeToTargetCurrency(context))
            .Aggregate(new Money(0m, context), MoneySum);

    private Money AggregateWithoutEvaluationContext()
        => ExceptionTransformer<InvalidOperationException>.Transform(
            AggregateSingleMoneyBag,
            exception => throw new MissingEvaluationContextException("Different currencies cannot be evaluated without an evaluation context.", exception));

    private Money AggregateSingleMoneyBag()
        => _currencies
            .SingleOrNone() // Single or None throws an InvalidOperationException if we have more than one currency in the Bag
            .Match(
                none: () => throw new Exception("nothing to Aggregate"),
                some: m => CheckAndAggregateBag(m.Value));

    private Money CheckAndAggregateBag(IEnumerable<Money> bag)
        => bag
            .Inspect(CheckEvaluationRules)
            .Aggregate(MoneySum);

    private void CheckEvaluationRules(Money money)
        => _roundingStrategy.Switch(
            none: () => _roundingStrategy = Option.Some(money.RoundingStrategy),
            some: r => CheckRoundingStrategy(money, r));

    private static void CheckRoundingStrategy(Money money, IRoundingStrategy roundingStrategy)
    {
        if (!money.RoundingStrategy.Equals(roundingStrategy))
        {
            throw new MissingEvaluationContextException("Different rounding strategies cannot be evaluated without an evaluation context.");
        }
    }

    private void CreateMoneyBagByCurrency(Currency currency)
    {
        if (!_currencies.ContainsKey(currency))
        {
            _currencies.Add(currency, new());
        }
    }

    private static Money MoneySum(Money currentSum, Money money)
        => currentSum with { Amount = currentSum.Amount + money.Amount };

    private static Func<Money, Money, Money> MoneySum(MoneyEvaluationContext context)
        => (currentSum, money)
            => new Money(currentSum.Amount + money.Amount, context);

    private static Func<Money, Money> ExchangeToTargetCurrency(MoneyEvaluationContext context)
        => money => money.Currency == context.TargetCurrency
            ? money
            : money with { Amount = money.Amount * context.Bank.ExchangeRate(money.Currency, context.TargetCurrency), Currency = context.TargetCurrency };
}

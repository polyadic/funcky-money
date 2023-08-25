using System.Numerics;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal sealed class MoneyBag<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    private readonly Dictionary<Currency, List<Money<TUnderlyingType>>> _currencies = new();
    private Option<IRoundingStrategy<TUnderlyingType>> _roundingStrategy;
    private Option<Currency> _emptyCurrency;

    public MoneyBag(Money<TUnderlyingType> money)
    {
        Add(money);
    }

    public MoneyBag<TUnderlyingType> Merge(MoneyBag<TUnderlyingType> moneyBag)
    {
        moneyBag
            ._currencies
            .SelectMany(kv => kv.Value)
            .ForEach(Add);

        return this;
    }

    public MoneyBag<TUnderlyingType> Multiply(TUnderlyingType factor)
    {
        _currencies
            .ForEach(kv => _currencies[kv.Key] = MultiplyBag(factor, kv.Value));

        return this;
    }

    public Money<TUnderlyingType> CalculateTotal(Option<MoneyEvaluationContext<TUnderlyingType>> context)
        => context.Match(
            none: AggregateWithoutEvaluationContext,
            some: AggregateWithEvaluationContext);

    private void Add(Money<TUnderlyingType> money)
    {
        if (money.IsZero)
        {
            if (_currencies.None())
            {
                _emptyCurrency = money.Currency;
            }
        }
        else
        {
            CreateMoneyBagByCurrency(money.Currency);
            _currencies[money.Currency].Add(money);
        }
    }

    private static List<Money<TUnderlyingType>> MultiplyBag(TUnderlyingType factor, IEnumerable<Money<TUnderlyingType>> bag)
        => bag
            .Select(m => m with { Amount = m.Amount * factor })
            .ToList();

    private Money<TUnderlyingType> AggregateWithEvaluationContext(MoneyEvaluationContext<TUnderlyingType> context)
        => _currencies
            .Values
            .Select(c => c.Aggregate(MoneySum(context)))
            .Select(ExchangeToTargetCurrency(context))
            .Aggregate(new Money<TUnderlyingType>(TUnderlyingType.Zero, context), MoneySum);

    private Money<TUnderlyingType> AggregateWithoutEvaluationContext()
        => ExceptionTransformer<InvalidOperationException>.Transform(
            AggregateSingleMoneyBag,
            exception => throw new MissingEvaluationContextException("Different currencies cannot be evaluated without an evaluation context.", exception));

    private Money<TUnderlyingType> AggregateSingleMoneyBag()
        => _currencies
            .SingleOrNone() // Single or None throws an InvalidOperationException if we have more than one currency in the Bag
            .Match(
                none: () => _emptyCurrency.Match(Money<TUnderlyingType>.Zero, c => Money<TUnderlyingType>.Zero with { Currency = c }),
                some: m => CheckAndAggregateBag(m.Value));

    private Money<TUnderlyingType> CheckAndAggregateBag(IEnumerable<Money<TUnderlyingType>> bag)
        => bag
            .Inspect(CheckEvaluationRules)
            .Aggregate(MoneySum);

    private void CheckEvaluationRules(Money<TUnderlyingType> money)
        => _roundingStrategy.Switch(
            none: () => _roundingStrategy = Option.Some(money.RoundingStrategy),
            some: r => CheckRoundingStrategy(money, r));

    private static void CheckRoundingStrategy(Money<TUnderlyingType> money, IRoundingStrategy<TUnderlyingType> roundingStrategy)
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

    private static Money<TUnderlyingType> MoneySum(Money<TUnderlyingType> currentSum, Money<TUnderlyingType> money)
        => currentSum with { Amount = currentSum.Amount + money.Amount };

    private static Func<Money<TUnderlyingType>, Money<TUnderlyingType>, Money<TUnderlyingType>> MoneySum(MoneyEvaluationContext<TUnderlyingType> context)
        => (currentSum, money)
            => new Money<TUnderlyingType>(currentSum.Amount + money.Amount, context);

    private static Func<Money<TUnderlyingType>, Money<TUnderlyingType>> ExchangeToTargetCurrency(MoneyEvaluationContext<TUnderlyingType> context)
        => money => money.Currency == context.TargetCurrency
            ? money
            : money with { Amount = money.Amount * context.Bank.ExchangeRate(money.Currency, context.TargetCurrency), Currency = context.TargetCurrency };
}

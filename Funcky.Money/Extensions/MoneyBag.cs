using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

internal sealed class MoneyBag
{
    private readonly Dictionary<Currency, List<MoneyExpression.Money>> _currencies = new();
    private Option<IRoundingStrategy> _roundingStrategy;
    private Option<Currency> _emptyCurrency;

    public MoneyBag(MoneyExpression.Money money)
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

    public MoneyExpression.Money CalculateTotal(Option<MoneyEvaluationContext> context)
        => context.Match(
            none: AggregateWithoutEvaluationContext,
            some: AggregateWithEvaluationContext);

    private void Add(MoneyExpression.Money money)
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

    private static List<MoneyExpression.Money> MultiplyBag(decimal factor, IEnumerable<MoneyExpression.Money> bag)
        => bag
            .Select(m => m with { Amount = m.Amount * factor })
            .ToList();

    private MoneyExpression.Money AggregateWithEvaluationContext(MoneyEvaluationContext context)
        => _currencies
            .Values
            .Select(c => c.Aggregate(MoneySum(context)))
            .Aggregate(new MoneyExpression.Money(0m, context), ToSingleCurrency(context));

    private MoneyExpression.Money AggregateWithoutEvaluationContext()
        => ExceptionTransformer<InvalidOperationException>.Transform(
            AggregateSingleMoneyBag,
            exception => throw new MissingEvaluationContextException("Different currencies cannot be evaluated without an evaluation context.", exception));

    private MoneyExpression.Money AggregateSingleMoneyBag()
        => _currencies
            .SingleOrNone() // Single or None throws an InvalidOperationException if we have more than one currency in the Bag
            .Match(
                none: () => _emptyCurrency.Match(MoneyExpression.Money.Zero, c => MoneyExpression.Money.Zero with { Currency = c }),
                some: m => CheckAndAggregateBag(m.Value));

    private MoneyExpression.Money CheckAndAggregateBag(IEnumerable<MoneyExpression.Money> bag)
        => bag
            .Inspect(CheckEvaluationRules)
            .Aggregate(MoneySum);

    private void CheckEvaluationRules(MoneyExpression.Money money)
        => _roundingStrategy.Match(
            none: () => _roundingStrategy = Option.Some(money.RoundingStrategy),
            some: r => CheckRoundingStrategy(money, r));

    private static void CheckRoundingStrategy(MoneyExpression.Money money, IRoundingStrategy roundingStrategy)
    {
        if (!money.RoundingStrategy.Equals(roundingStrategy))
        {
            throw new MissingEvaluationContextException("Different rounding strategies cannot be evaluated without an evaluation context.");
        }
    }

    private static Func<MoneyExpression.Money, MoneyExpression.Money, MoneyExpression.Money> ToSingleCurrency(MoneyEvaluationContext context)
        => (moneySum, money)
            => moneySum with { Amount = moneySum.Amount + ExchangeToTargetCurrency(money, context).Amount };

    private void CreateMoneyBagByCurrency(Currency currency)
    {
        if (!_currencies.ContainsKey(currency))
        {
            _currencies.Add(currency, new());
        }
    }

    private static MoneyExpression.Money MoneySum(MoneyExpression.Money currentSum, MoneyExpression.Money money)
        => currentSum with { Amount = currentSum.Amount + money.Amount };

    private static Func<MoneyExpression.Money, MoneyExpression.Money, MoneyExpression.Money> MoneySum(MoneyEvaluationContext context)
        => (currentSum, money)
            => new MoneyExpression.Money(currentSum.Amount + money.Amount, context);

    private static MoneyExpression.Money ExchangeToTargetCurrency(MoneyExpression.Money money, MoneyEvaluationContext context)
        => money.Currency == context.TargetCurrency
            ? money
            : money with { Amount = money.Amount * context.Bank.ExchangeRate(money.Currency, context.TargetCurrency), Currency = context.TargetCurrency };
}

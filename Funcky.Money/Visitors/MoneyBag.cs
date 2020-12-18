using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class MoneyBag
    {
        private readonly Dictionary<Currency, List<Money>> _currencies = new();
        private Option<IRoundingStrategy> _roundingStrategy;
        private Option<Currency> _firstCurrency;

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
                none: AggregateWithEvaluationContext,
                some: AggregateWithoutEvaluationContext);

        private void Add(Money money)
        {
            if (money.IsZero)
            {
                if (_currencies.None())
                {
                    _firstCurrency = money.Currency;
                }
            }
            else
            {
                CreateMoneyBagByCurrency(money.Currency);
                _currencies[money.Currency].Add(money);
            }
        }

        private static List<Money> MultiplyBag(decimal factor, IEnumerable<Money> bag)
            => bag
                .Select(m => m with { Amount = m.Amount * factor })
                .ToList();

        private Money AggregateWithoutEvaluationContext(MoneyEvaluationContext context)
            => _currencies
                .Select(kv => kv.Value.Aggregate(MoneySum(context)))
                .Aggregate(new Money(0m, context), ToSingleCurrency(context));

        private Money AggregateWithEvaluationContext()
            => ExceptionTransformer<InvalidOperationException>.Transform(
                AggregateSingleMoneyBag,
                exception => throw new MissingEvaluationContextException("Different currencies cannot be evaluated without an evaluation context.", exception));

        private Money AggregateSingleMoneyBag()
            => _currencies
                .SingleOrNone()
                .Match(
                    none: () => _firstCurrency.Match(Money.Zero, c => Money.Zero with { Currency = c }),
                    some: m => CheckAndAggregateBag(m.Value));

        private Money CheckAndAggregateBag(IEnumerable<Money> bag)
            => bag
                .Inspect(CheckEvaluationRules)
                .Aggregate(MoneySum);

        private void CheckEvaluationRules(Money money) =>
            _roundingStrategy.Match(
                none: () => _roundingStrategy = Option.Some(money.RoundingStrategy),
                some: r => CheckRoundingStrategy(money.RoundingStrategy.Equals(r)));

        private static void CheckRoundingStrategy(bool validRoundingStrategy)
        {
            if (!validRoundingStrategy)
            {
                throw new MissingEvaluationContextException("Different rounding strategies cannot be evaluated without an evaluation context.");
            }
        }

        private static Func<Money, Money, Money> ToSingleCurrency(MoneyEvaluationContext context)
            => (moneySum, money)
                => moneySum with { Amount = moneySum.Amount + ExchangeToTargetCurrency(money, context).Amount };

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

        private static Money ExchangeToTargetCurrency(Money money, MoneyEvaluationContext context)
            => money.Currency == context.TargetCurrency
                ? money
                : money with { Amount = money.Amount * context.Bank.ExchangeRate(money.Currency, context.TargetCurrency), Currency = context.TargetCurrency };
    }
}

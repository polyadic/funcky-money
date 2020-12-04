using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class MoneyBags
    {
        private readonly Dictionary<Currency, List<Money>> _moneyBags = new();
        private Option<AbstractRoundingStrategy> _roundingStrategy;

        public void Add(Money money)
        {
            if (!money.IsZero)
            {
                CreateMoneyBagByCurrency(money);

                _moneyBags[money.Currency].Add(money);
            }
        }

        public void Multiply(decimal factor)
            => _moneyBags.ForEach(kv => _moneyBags[kv.Key] = MultiplyBag(factor, kv.Value));

        public Money CalculateTotal(Option<MoneyEvaluationContext> context)
            => context.Match(
                none: AggregateWithEvaluationContext,
                some: AggregateWithoutEvaluationContext);

        public void Clear()
            => _moneyBags.Clear();

        private List<Money> MultiplyBag(decimal factor, IEnumerable<Money> bag) =>
            bag
                .Select(m => m with { Amount = m.Amount * factor })
                .ToList();

        private Money AggregateWithoutEvaluationContext(MoneyEvaluationContext context)
            => _moneyBags
                .Select(kv => kv.Value.Aggregate(MoneySum(context)))
                .Aggregate(new Money(0m, context), ToSingleCurrency(context));

        private Money AggregateWithEvaluationContext()
            => ExceptionTransformer<InvalidOperationException>.Transform(
                AggregateSingleMoneyBag,
                exception => throw new MissingEvaluationContextException("Different currencies cannot be evaluated without an evaluation context.", exception));

        private Money AggregateSingleMoneyBag()
            => _moneyBags
                .SingleOrNone()
                .Match(
                    none: () => Money.Zero,
                    some: m => CheckAndAggregateBag(m.Value));

        private Money CheckAndAggregateBag(List<Money> bag)
            => bag
                .Inspect(CheckEvaluationRules)
                .Aggregate(MoneySum);

        private void CheckEvaluationRules(Money money)
        {
            _roundingStrategy.Match(
                none: () => _roundingStrategy = Option.Some(money.RoundingStrategy),
                some: r => CheckRoundingStrategy(r == money.RoundingStrategy));
        }

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

        private void CreateMoneyBagByCurrency(Money money)
        {
            if (!_moneyBags.ContainsKey(money.Currency))
            {
                _moneyBags.Add(money.Currency, new List<Money>());
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

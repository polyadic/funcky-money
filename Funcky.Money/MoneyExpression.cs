using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky;

[DiscriminatedUnion]
public abstract partial record MoneyExpression
{
    public static MoneyExpression operator *(MoneyExpression multiplicand, decimal multiplier)
        => multiplicand.Multiply(multiplier);

    public static MoneyExpression operator *(decimal multiplier, MoneyExpression multiplicand)
        => multiplicand.Multiply(multiplier);

    public static MoneyExpression operator /(MoneyExpression dividend, decimal divisor)
        => dividend.Divide(divisor);

    public static MoneyExpression operator +(MoneyExpression augend, MoneyExpression addend)
        => augend.Add(addend);

    public static MoneyExpression operator +(MoneyExpression moneyExpression)
        => moneyExpression;

    public static MoneyExpression operator -(MoneyExpression minuend, MoneyExpression subtrahend)
        => minuend.Subtract(subtrahend);

    public static MoneyExpression operator -(MoneyExpression moneyExpression)
        => moneyExpression.Multiply(-1);

    [DebuggerDisplay("{Amount} {Currency.AlphabeticCurrencyCode,nq}")]
    public partial record Money : MoneyExpression
    {
        public static readonly Money Zero = new(0m);

        public Money(decimal amount, Option<Currency> currency = default)
        {
            Amount = amount;
            Currency = SelectCurrency(currency);
            RoundingStrategy = Funcky.RoundingStrategy.Default(Currency);
        }

        public Money(decimal amount, MoneyEvaluationContext context)
        {
            Amount = amount;
            Currency = context.TargetCurrency;
            RoundingStrategy = context.RoundingStrategy;
        }

        public Money(int amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public decimal Amount { get; init; }

        public Currency Currency { get; init; }

        public IRoundingStrategy RoundingStrategy { get; }

        public bool IsZero
            => Amount == 0m;

        // These operators supports the operators on MoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
        public static MoneyExpression operator +(Money augend, MoneyExpression addend)
            => augend.Add(addend);

        public static MoneyExpression operator +(Money money)
            => money;

        public static MoneyExpression operator -(Money minuend, MoneyExpression subtrahend)
            => minuend.Subtract(subtrahend);

        public static Money operator -(Money money)
            => money with { Amount = -money.Amount };

        public static MoneyExpression operator *(Money multiplicand, decimal multiplier)
            => multiplicand.Multiply(multiplier);

        public static MoneyExpression operator *(decimal multiplier, Money multiplicand)
            => multiplicand.Multiply(multiplier);

        public static MoneyExpression operator /(Money dividend, decimal divisor)
            => dividend.Divide(divisor);

        private static Currency SelectCurrency(Option<Currency> currency)
            => currency.GetOrElse(CurrencyCulture.CurrentCurrency);

        public override string ToString()
            => CurrencyCulture.FormatProviderFromCurrency(Currency).Match(
                none: () => string.Format($"{{0:N{Currency.MinorUnitDigits}}} {{1}}", Amount, Currency.AlphabeticCurrencyCode),
                some: formatProvider => string.Format(formatProvider, $"{{0:C{Currency.MinorUnitDigits}}}", Amount));
    }

    public sealed partial record MoneySum : MoneyExpression
    {
        public MoneySum(MoneyExpression leftMoneyExpression, MoneyExpression rightMoneyExpression)
        {
            Left = leftMoneyExpression;
            Right = rightMoneyExpression;
        }

        public MoneyExpression Left { get; }

        public MoneyExpression Right { get; }
    }

    public sealed partial record MoneyProduct : MoneyExpression
    {
        public MoneyProduct(MoneyExpression moneyExpression, decimal factor)
        {
            Expression = moneyExpression;
            Factor = factor;
        }

        public MoneyExpression Expression { get; }

        public decimal Factor { get; }
    }

    public sealed partial record MoneyDistribution : IEnumerable<MoneyExpression>
    {
        public MoneyDistribution(MoneyExpression moneyExpression, IEnumerable<int> factors, Option<decimal> precision)
        {
            Expression = moneyExpression;
            Factors = factors.ToList();
            Precision = precision;

            if (Factors.None())
            {
                throw new ImpossibleDistributionException("we need at least one factor to distribute.");
            }
        }

        public MoneyExpression Expression { get; }

        public List<int> Factors { get; }

        public Option<decimal> Precision { get; }

        public IEnumerator<MoneyExpression> GetEnumerator()
            => Factors
                .WithIndex()
                .Select(f => (MoneyExpression)new MoneyExpression.MoneyDistributionPart(this, f.Index))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    public sealed partial record MoneyDistributionPart : MoneyExpression
    {
        public MoneyDistributionPart(MoneyDistribution distribution, int index)
        {
            Distribution = distribution;
            Index = index;
        }

        public MoneyDistribution Distribution { get; }

        public int Index { get; }
    }
}

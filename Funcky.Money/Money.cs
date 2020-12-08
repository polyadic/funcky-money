using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    public sealed partial record Money : IMoneyExpression
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

        public static Option<Money> ParseOrNone(string money, Option<Currency> currency = default)
        {
            var selectedCurrency = SelectCurrency(currency);

            return money
                .TryParseDecimal(NumberStyles.Currency, CurrencyCulture.CultureInfoFromCurrency(selectedCurrency))
                .AndThen(m => new Money(m, selectedCurrency));
        }

        public override string ToString()
            => string.Format(CurrencyCulture.CultureInfoFromCurrency(Currency), "{0:C}", Amount);

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);

        // These operators supports the operators on IMoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
        public static IMoneyExpression operator +(Money augend, IMoneyExpression addend)
            => augend.Add(addend);

        public static IMoneyExpression operator +(Money money)
            => money;

        public static IMoneyExpression operator -(Money minuend, IMoneyExpression subtrahend)
            => minuend.Subtract(subtrahend);

        public static Money operator -(Money money)
            => money with { Amount = -money.Amount };

        public static IMoneyExpression operator *(Money multiplicand, decimal multiplier)
            => multiplicand.Multiply(multiplier);

        public static IMoneyExpression operator *(decimal multiplier, Money multiplicand)
            => multiplicand.Multiply(multiplier);

        public static IMoneyExpression operator /(Money dividend, decimal divisor)
            => dividend.Divide(divisor);

        private static Currency SelectCurrency(Option<Currency> currency)
            => currency.GetOrElse(CurrencyCulture.CurrentCurrency);
    }
}

using System.Globalization;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
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

        public AbstractRoundingStrategy RoundingStrategy { get; }

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
        public static IMoneyExpression operator *(Money moneyExpression, decimal factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator *(decimal factor, Money moneyExpression)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator +(Money leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => new MoneySum(leftMoneyExpression, rightMoneyExpression);

        private static Currency SelectCurrency(Option<Currency> currency)
            => currency.GetOrElse(CurrencyCulture.CurrentCurrency);

        // ReSharper disable InconsistentNaming - Reason: we want the currencies in capital letters
        public static Money CHF(decimal amount)
            => new(amount, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.CHF()).WithRounding(Funcky.RoundingStrategy.BankersRounding(0.05m)).Build());

        public static Money EUR(decimal amount)
            => new(amount, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.EUR()).Build());

        public static Money USD(decimal amount)
            => new(amount, MoneyEvaluationContext.Builder.Default.WithTargetCurrency(Currency.USD()).Build());
    }
}

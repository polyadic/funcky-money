using System;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
        public static readonly Money Zero = new Money(0m);

        public Money(decimal amount, Option<Currency> currency = default)
        {
            Currency = currency.GetOrElse(() => CurrencyCulture.CurrentCurrency());
            Amount = Math.Round(amount, Currency.MinorUnitDigits, MidpointRounding.ToEven);
        }

        public Money(int amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public Money(double amount, Option<Currency> currency = default)
            : this((decimal)amount, currency)
        {
        }

        public decimal Amount { get; }

        public Currency Currency { get; }

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);

        // These operators supports the operators on IMoneyExpression, because Money + Money or Money * factor does not work otherwise without a cast.
        public static IMoneyExpression operator *(Money moneyExpression, decimal factor)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator *(decimal factor, Money moneyExpression)
            => new MoneyProduct(moneyExpression, factor);

        public static IMoneyExpression operator +(Money leftMoneyExpression, IMoneyExpression rightMoneyExpression)
            => new MoneySum(leftMoneyExpression, rightMoneyExpression);
    }
}

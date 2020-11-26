using System;
using Funcky.Monads;

namespace Funcky
{
    public record Money : IMoneyExpression
    {
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
    }
}

using System.Diagnostics.CodeAnalysis;

#pragma warning disable SA1649 // SA1649FileNameMustMatchTypeName : Records are not yet supported by StyleCop

namespace Funcky
{
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201", Justification = "Records are not yet supported by StyleCop")]
    public record Money : IMoneyExpression
    {
        public Money(decimal amount)
        {
            Amount = amount;
            Currency = Currency.CHF();
        }

        public decimal Amount { get; }
        public Currency Currency { get; }
    }
}

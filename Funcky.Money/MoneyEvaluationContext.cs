using System.Collections.Immutable;

namespace Funcky;

public sealed record MoneyEvaluationContext
{
    private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> smallestDistributionUnit, IBank bank)
        => (TargetCurrency, SmallestDistributionUnit, Bank) = (targetCurrency, smallestDistributionUnit, bank);

    public Currency TargetCurrency { get; }

    /// <summary>The unit in which money gets distributed; the minor unit of the currency when not set.</summary>
    public Option<decimal> SmallestDistributionUnit { get; }

    public IBank Bank { get; }

    /// <summary>The factor to convert an amount of <paramref name="source"/> into the target currency.</summary>
    public decimal ExchangeRate(Currency source)
        => Bank.ExchangeRate(source, TargetCurrency);

    public sealed record Builder
    {
        public static readonly Builder Default = new();

        private Builder()
        {
        }

        private Option<Currency> TargetCurrency { get; init; }

        private Option<decimal> SmallestDistributionUnit { get; init; }

        private ImmutableDictionary<Currency, decimal> ExchangeRates { get; init; } = ImmutableDictionary<Currency, decimal>.Empty;

        public Builder WithTargetCurrency(Currency currency)
            => this with { TargetCurrency = currency };

        public Builder WithSmallestDistributionUnit(decimal smallestDistributionUnit)
            => this with { SmallestDistributionUnit = smallestDistributionUnit };

        /// <summary>Defines the factor to convert an amount of <paramref name="currency"/> into the target currency.</summary>
        public Builder WithExchangeRate(Currency currency, decimal exchangeRate)
            => this with { ExchangeRates = ExchangeRates.SetItem(currency, exchangeRate) };

        public MoneyEvaluationContext Build()
        {
            var targetCurrency = TargetCurrency.GetOrElse(() => throw new InvalidMoneyEvaluationContextBuilderException("Target currency must be set."));

            return new(targetCurrency, SmallestDistributionUnit, new DefaultBank(targetCurrency, ExchangeRates));
        }
    }
}

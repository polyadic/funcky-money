using System.Collections.Immutable;

namespace Funcky;

public sealed record MoneyEvaluationContext
{
    private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> smallestDistributionUnit, IRoundingStrategy roundingStrategy, IBank bank)
        => (TargetCurrency, SmallestDistributionUnit, RoundingStrategy, Bank) = (targetCurrency, smallestDistributionUnit, roundingStrategy, bank);

    public Currency TargetCurrency { get; }

    /// <summary>The unit in which money gets distributed; the minor unit of the currency when not set.</summary>
    public Option<decimal> SmallestDistributionUnit { get; }

    /// <summary>How the result of an evaluation is rounded; bankers rounding to the smallest distribution unit by default.</summary>
    public IRoundingStrategy RoundingStrategy { get; }

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

        private Option<IRoundingStrategy> RoundingStrategy { get; init; }

        private ImmutableDictionary<Currency, decimal> ExchangeRates { get; init; } = ImmutableDictionary<Currency, decimal>.Empty;

        public Builder WithTargetCurrency(Currency currency)
            => this with { TargetCurrency = currency };

        public Builder WithSmallestDistributionUnit(decimal smallestDistributionUnit)
            => this with { SmallestDistributionUnit = smallestDistributionUnit };

        public Builder WithRounding(IRoundingStrategy roundingStrategy)
            => this with { RoundingStrategy = Option.Some(roundingStrategy) };

        /// <summary>Defines the factor to convert an amount of <paramref name="currency"/> into the target currency.</summary>
        public Builder WithExchangeRate(Currency currency, decimal exchangeRate)
            => this with { ExchangeRates = ExchangeRates.SetItem(currency, exchangeRate) };

        public MoneyEvaluationContext Build()
        {
            var targetCurrency = TargetCurrency.GetOrElse(() => throw new InvalidMoneyEvaluationContextBuilderException("Target currency must be set."));

            return new(
                targetCurrency,
                SmallestDistributionUnit,
                RoundingStrategy.GetOrElse(() => DefaultRounding(targetCurrency)),
                new DefaultBank(targetCurrency, ExchangeRates));
        }

        private IRoundingStrategy DefaultRounding(Currency targetCurrency)
            => SmallestDistributionUnit.Match(
                none: () => Funcky.RoundingStrategy.Default(targetCurrency),
                some: Funcky.RoundingStrategy.Default);
    }
}

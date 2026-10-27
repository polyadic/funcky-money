namespace Funcky;

public sealed record MoneyEvaluationContext
{
    private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> smallestDistributionUnit)
        => (TargetCurrency, SmallestDistributionUnit) = (targetCurrency, smallestDistributionUnit);

    public Currency TargetCurrency { get; }

    /// <summary>The unit in which money gets distributed; the minor unit of the currency when not set.</summary>
    public Option<decimal> SmallestDistributionUnit { get; }

    public sealed record Builder
    {
        public static readonly Builder Default = new();

        private Builder()
        {
        }

        private Option<Currency> TargetCurrency { get; init; }

        private Option<decimal> SmallestDistributionUnit { get; init; }

        public Builder WithTargetCurrency(Currency currency)
            => this with { TargetCurrency = currency };

        public Builder WithSmallestDistributionUnit(decimal smallestDistributionUnit)
            => this with { SmallestDistributionUnit = smallestDistributionUnit };

        public MoneyEvaluationContext Build()
            => new(
                TargetCurrency.GetOrElse(() => throw new InvalidMoneyEvaluationContextBuilderException("Target currency must be set.")),
                SmallestDistributionUnit);
    }
}

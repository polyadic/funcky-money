namespace Funcky;

public sealed record MoneyEvaluationContext
{
    private MoneyEvaluationContext(Currency targetCurrency, decimal smallestDistributionUnit)
        => (TargetCurrency, SmallestDistributionUnit) = (targetCurrency, smallestDistributionUnit);

    public Currency TargetCurrency { get; }

    public decimal SmallestDistributionUnit { get; }

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
                SmallestDistributionUnit.GetOrElse(0.01m));
    }
}

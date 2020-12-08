using System;
using Funcky.Monads;

namespace Funcky
{
    public sealed class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> distributionUnit, Option<IRoundingStrategy> roundingStrategy, IBank bank)
        {
            TargetCurrency = targetCurrency;
            DistributionUnit = distributionUnit;
            RoundingStrategy = roundingStrategy
                .GetOrElse(Funcky.RoundingStrategy.Default(distributionUnit.GetOrElse(Power.OfATenth(TargetCurrency.MinorUnitDigits))));
            Bank = bank;
        }

        public Currency TargetCurrency { get; }

        public Option<decimal> DistributionUnit { get; }

        public IRoundingStrategy RoundingStrategy { get; }

        public IBank Bank { get; }

        public sealed class Builder
        {
            public static readonly Builder Default = new();

            private readonly Option<Currency> _targetCurrency;
            private readonly Option<decimal> _distributionUnit;
            private readonly Option<IRoundingStrategy> _roundingStrategy;
            private readonly IBank _bank;

            private Builder()
            {
                _targetCurrency = default;
                _roundingStrategy = default;
                _distributionUnit = default;
                _bank = DefaultBank.Empty;
            }

            private Builder(Option<Currency> currency, Option<decimal> distributionUnit, Option<IRoundingStrategy> roundingStrategy, IBank bank)
            {
                _targetCurrency = currency;
                _distributionUnit = distributionUnit;
                _roundingStrategy = roundingStrategy;
                _bank = bank;
            }

            public MoneyEvaluationContext Build()
            {
                if (CompatibleRounding().Match(false, c => !c))
                {
                    throw new IncompatibleRoundingException($"The roundingStrategy {_roundingStrategy} is incompatible with the smallest possible distribution unit {_distributionUnit}.");
                }

                return CreateContext();
            }

            public Builder WithTargetCurrency(Currency currency)
                => new(currency, _distributionUnit, _roundingStrategy, _bank);

            public Builder WithRounding(IRoundingStrategy roundingStrategy)
                => new(_targetCurrency, _distributionUnit, Option.Some(roundingStrategy), _bank);

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
                => _bank is DefaultBank bank
                    ? new(_targetCurrency, _distributionUnit, _roundingStrategy, bank.AddExchangeRate(currency, GetTargetCurrencyOrException(), sellRate))
                    : throw new InvalidMoneyEvaluationContextBuilderException("You can either use WithExchangeRate or WithBank, but not both.");

            public Builder WithBank(IBank bank)
                => new(_targetCurrency, _distributionUnit, _roundingStrategy, bank);

            public Builder WithSmallestDistributionUnit(in decimal distributionUnit)
                => new(_targetCurrency, distributionUnit, _roundingStrategy, _bank);

            private Currency GetTargetCurrencyOrException()
                => _targetCurrency.GetOrElse(()
                    => throw new InvalidMoneyEvaluationContextBuilderException("You need to set a target currency before you can add an exchange rate."));

            private Option<bool> CompatibleRounding()
                => from rounding in _roundingStrategy
                   from unit in _distributionUnit
                   select rounding.IsSameAfterRounding(unit);

            private MoneyEvaluationContext CreateContext()
                => new(
                    _targetCurrency.GetOrElse(() =>
                        throw new InvalidMoneyEvaluationContextBuilderException("Money evaluation context has no target currency set.")),
                    _distributionUnit,
                    _roundingStrategy,
                    _bank);
        }
    }
}

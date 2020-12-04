using System;
using Funcky.Monads;

namespace Funcky
{
    public sealed class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, Option<AbstractRoundingStrategy> roundingStrategy, IBank bank)
        {
            TargetCurrency = targetCurrency;
            RoundingStrategy = roundingStrategy.GetOrElse(Funcky.RoundingStrategy.Default(TargetCurrency));
            Bank = bank;
        }

        public Currency TargetCurrency { get; }

        public AbstractRoundingStrategy RoundingStrategy { get; }

        public IBank Bank { get; }

        public sealed class Builder
        {
            public static readonly Builder Default = new();

            private readonly Option<Currency> _targetCurrency;
            private readonly Option<AbstractRoundingStrategy> _roundingStrategy;
            private readonly IBank _bank;

            private Builder()
            {
                _targetCurrency = default;
                _roundingStrategy = default;
                _bank = DefaultBank.Empty;
            }

            private Builder(Option<Currency> currency, Option<AbstractRoundingStrategy> roundingStrategy, IBank bank)
            {
                _targetCurrency = currency;
                _roundingStrategy = roundingStrategy;
                _bank = bank;
            }

            public MoneyEvaluationContext Build()
                => new(
                    _targetCurrency.GetOrElse(() => throw new Exception("Money evaluation context has no target currency set.")),
                    _roundingStrategy,
                    _bank);

            public Builder WithTargetCurrency(Currency currency)
                => new(currency, _roundingStrategy, _bank);

            public Builder WithRounding(AbstractRoundingStrategy roundingStrategy)
                => new(_targetCurrency, roundingStrategy, _bank);

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
                => _bank is DefaultBank bank
                    ? new(_targetCurrency, _roundingStrategy, bank.AddExchangeRate(currency, GetTargetCurrencyOrException(), sellRate))
                    : throw new Exception("Invalid Builder: you can either use WithExchangeRate or WithBank, but not both.");

            public Builder WithBank(IBank bank)
                => new(_targetCurrency, _roundingStrategy, bank);

            private Currency GetTargetCurrencyOrException()
                => _targetCurrency.GetOrElse(()
                    => throw new Exception("You need to set a target currency before you can add an exchange rate."));
        }
    }
}

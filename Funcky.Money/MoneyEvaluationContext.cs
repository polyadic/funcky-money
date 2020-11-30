using System;
using Funcky.Monads;

namespace Funcky
{
    public class MoneyEvaluationContext
    {
        private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> precision, Option<MidpointRounding> midpointRounding, IBank bank)
        {
            TargetCurrency = targetCurrency;
            Precision = precision.GetOrElse(() => Power.OfATenth(targetCurrency.MinorUnitDigits));
            MidpointRounding = midpointRounding.GetOrElse(RoundingStrategy.DefaultRoundingStrategy);
            Bank = bank;
        }

        public Currency TargetCurrency { get; }

        public decimal Precision { get; }

        public MidpointRounding MidpointRounding { get; }

        public IBank Bank { get; }

        public class Builder
        {
            public static readonly Builder Default = new();

            private readonly Option<Currency> _targetCurrency;
            private readonly Option<decimal> _precision;
            private readonly Option<MidpointRounding> _midpointRounding;
            private readonly IBank _bank;

            private Builder()
            {
                _targetCurrency = default;
                _precision = default;
                _midpointRounding = default;
                _bank = DefaultBank.Empty;
            }

            private Builder(Option<Currency> currency, Option<decimal> precision, Option<MidpointRounding> midpointRounding, IBank bank)
            {
                _targetCurrency = currency;
                _precision = precision;
                _midpointRounding = midpointRounding;
                _bank = bank;
            }

            public MoneyEvaluationContext Build()
                => new(
                    _targetCurrency.GetOrElse(() => throw new Exception("Money evaluation context has no target currency set.")),
                    _precision,
                    _midpointRounding,
                    _bank);

            public Builder WithTargetCurrency(Currency currency)
                => new(currency, _precision, _midpointRounding, _bank);

            public Builder WithPrecision(decimal precision)
                => new(_targetCurrency, precision, _midpointRounding, _bank);

            public Builder WithMidpointRounding(decimal precision)
                => new(_targetCurrency, precision, _midpointRounding, _bank);

            public Builder WithExchangeRate(Currency currency, decimal sellRate)
                => _bank is DefaultBank bank
                    ? new(_targetCurrency, _precision, _midpointRounding, bank.AddExchangeRate(currency, GetTargetCurrencyOrException(), sellRate))
                    : throw new Exception("Invalid Builder: you can either use WithExchangeRate or WithBank, but not both.");

            public Builder WithBank(IBank bank)
                => new(_targetCurrency, _precision, _midpointRounding, bank);

            private Currency GetTargetCurrencyOrException()
                => _targetCurrency.GetOrElse(()
                    => throw new Exception("You need to set a target currency before you can add an exchange rate."));
        }
    }
}

using Funcky.Monads;
using static Funcky.Functional;

namespace Funcky;

public sealed class MoneyEvaluationContext
{
    private MoneyEvaluationContext(Currency targetCurrency, Option<decimal> distributionUnit, Option<IRoundingStrategy<decimal>> roundingStrategy, IBank<decimal> bank)
    {
        TargetCurrency = targetCurrency;
        DistributionUnit = distributionUnit;
        RoundingStrategy = roundingStrategy.GetOrElse(Funcky.RoundingStrategy.Default(distributionUnit.GetOrElse(Power<decimal>.OfATenth(TargetCurrency.MinorUnitDigits))));
        Bank = bank;
    }

    /// <summary>
    /// If there are multiple currencies involved in an evaluation, we will convert all the money amounts of different currencies to this one.
    /// </summary>
    public Currency TargetCurrency { get; }

    /// <summary>
    /// This is the smallest money unit you want to have distributed.
    /// Usually you want this to be the smallest coin of the given currency.
    /// </summary>
    public Option<decimal> DistributionUnit { get; }

    /// <summary>
    /// Defines how we round amounts in the evaluation.
    /// </summary>
    public IRoundingStrategy<decimal> RoundingStrategy { get; }

    /// <summary>
    /// Source for exchange rates.
    /// </summary>
    public IBank<decimal> Bank { get; }

    public sealed class Builder
    {
        public static readonly Builder Default = new();

        private readonly Option<Currency> _targetCurrency;
        private readonly Option<decimal> _distributionUnit;
        private readonly Option<IRoundingStrategy<decimal>> _roundingStrategy;
        private readonly IBank<decimal> _bank;

        private Builder()
        {
            _targetCurrency = default;
            _roundingStrategy = default;
            _distributionUnit = default;
            _bank = DefaultBank<decimal>.Empty;
        }

        private Builder(Option<Currency> currency, Option<decimal> distributionUnit, Option<IRoundingStrategy<decimal>> roundingStrategy, IBank<decimal> bank)
        {
            _targetCurrency = currency;
            _distributionUnit = distributionUnit;
            _roundingStrategy = roundingStrategy;
            _bank = bank;
        }

        public MoneyEvaluationContext Build()
            => CompatibleRounding().Match(none: false, some: Not<bool>(Identity))
                ? throw new IncompatibleRoundingException($"The rounding strategy {_roundingStrategy} is incompatible with the smallest possible distribution unit {_distributionUnit}.")
                : CreateContext();

        public Builder WithTargetCurrency(Currency currency)
            => With(targetCurrency: currency);

        public Builder WithRounding(IRoundingStrategy<decimal> roundingStrategy)
            => With(roundingStrategy: Option.Some(roundingStrategy));

        public Builder WithExchangeRate(Currency currency, decimal sellRate)
            => _bank is DefaultBank<decimal> bank
                ? With(bank: bank.AddExchangeRate(currency, GetTargetCurrencyOrException(), sellRate))
                : throw new InvalidMoneyEvaluationContextBuilderException("You can either use WithExchangeRate or WithBank, but not both.");

        public Builder WithBank(IBank<decimal> bank)
            => With(bank: Option.Some(bank));

        public Builder WithSmallestDistributionUnit(decimal distributionUnit)
            => With(distributionUnit: distributionUnit);

        private Builder With(Option<Currency> targetCurrency = default, Option<decimal> distributionUnit = default, Option<IRoundingStrategy<decimal>> roundingStrategy = default, Option<IBank<decimal>> bank = default)
            => new(
                targetCurrency.OrElse(_targetCurrency),
                distributionUnit.OrElse(_distributionUnit),
                roundingStrategy.OrElse(_roundingStrategy),
                bank.GetOrElse(_bank));

        private Currency GetTargetCurrencyOrException()
            => _targetCurrency.GetOrElse(()
                => throw new InvalidMoneyEvaluationContextBuilderException("You need to set a target currency before you can add an exchange rate."));

        private Option<bool> CompatibleRounding()
            => from rounding in _roundingStrategy
               from unit in _distributionUnit
               select rounding.IsSameAfterRounding(unit);

        private MoneyEvaluationContext CreateContext()
            => new(
                _targetCurrency.GetOrElse(() => throw new InvalidMoneyEvaluationContextBuilderException("Money evaluation context has no target currency set.")),
                _distributionUnit,
                _roundingStrategy,
                _bank);
    }
}

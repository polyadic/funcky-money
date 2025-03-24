using Xunit;

namespace Funcky.Test;

public sealed class RoundingStrategyTest
{
    [Theory]
    [InlineData(4.2249)]
    [InlineData(4.22494)]
    [InlineData(4.22495)]
    [InlineData(4.225)]
    [InlineData(4.22501)]
    [InlineData(4.2749)]
    [InlineData(4.27494)]
    [InlineData(4.27495)]
    [InlineData(4.27499)]
    [InlineData(4.275)]
    [InlineData(4.27501)]
    public void GivenANoRoundingContextTheNumbersGetNotRounded(decimal originalValue)
    {
        var sellingPriceRoundingEvaluationContext = GetEvaluationContext(RoundingStrategy.NoRounding());

        var originalMoneyValue = new Money(originalValue, Currency.CHF);
        var roundedMoneyValue = originalMoneyValue.Evaluate(sellingPriceRoundingEvaluationContext);

        Assert.Equal(originalMoneyValue.Amount, roundedMoneyValue.Amount);
    }

    [Theory]
    [InlineData(4.2249, 4.20)]
    [InlineData(4.22494, 4.20)]
    [InlineData(4.22495, 4.20)]
    [InlineData(4.225, 4.20)]
    [InlineData(4.22501, 4.25)]
    [InlineData(4.2749, 4.25)]
    [InlineData(4.27494, 4.25)]
    [InlineData(4.27495, 4.25)]
    [InlineData(4.27499, 4.25)]
    [InlineData(4.275, 4.30)]
    [InlineData(4.27501, 4.30)]
    public void GivenABankersRoundingContextTheNumbersGetRoundedAccordingToBankersRounding(decimal originalValue, decimal expectedRoundedValue)
    {
        var sellingPriceRoundingEvaluationContext = GetEvaluationContext(RoundingStrategy.BankersRounding(0.05m));

        var originalMoneyValue = new Money(originalValue, Currency.CHF);
        var roundedMoneyValue = originalMoneyValue.Evaluate(sellingPriceRoundingEvaluationContext);

        Assert.Equal(expectedRoundedValue, roundedMoneyValue.Amount);
    }

    [Theory]
    [InlineData(4.2249, 4.20)]
    [InlineData(4.22494, 4.20)]
    [InlineData(4.22495, 4.20)]
    [InlineData(4.225, 4.25)]
    [InlineData(4.22501, 4.25)]
    [InlineData(4.2749, 4.25)]
    [InlineData(4.27494, 4.25)]
    [InlineData(4.27495, 4.25)]
    [InlineData(4.27499, 4.25)]
    [InlineData(4.275, 4.30)]
    [InlineData(4.27501, 4.30)]
    public void GivenAnAwayFromZeroRoundingContextTheNumbersGetRoundedAwayFromZero(decimal originalValue, decimal expectedRoundedValue)
    {
        var sellingPriceRoundingEvaluationContext = GetEvaluationContext(RoundingStrategy.RoundWithAwayFromZero(0.05m));

        var originalMoneyValue = new Money(originalValue, Currency.CHF);
        var roundedMoneyValue = originalMoneyValue.Evaluate(sellingPriceRoundingEvaluationContext);

        Assert.Equal(expectedRoundedValue, roundedMoneyValue.Amount);
    }

    private static MoneyEvaluationContext GetEvaluationContext(IRoundingStrategy roundingStrategy)
        => MoneyEvaluationContext.Builder.
            Default
            .WithTargetCurrency(Currency.CHF)
            .WithRounding(roundingStrategy)
            .Build();
}

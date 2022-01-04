using Funcky.Monads;

namespace Funcky;

public static class MoneyEvaluationExtension
{
    public static MoneyExpression.Money Evaluate(this MoneyExpression moneyExpression, Option<MoneyEvaluationContext> context = default)
        => Evaluate(moneyExpression, Round(context), CalculateTotal(context));

    private static MoneyExpression.Money Evaluate(MoneyExpression moneyExpression, Func<MoneyExpression.Money, MoneyExpression.Money> round, Func<MoneyExpression, MoneyExpression.Money> total)
        => round(total(moneyExpression));

    private static Func<MoneyExpression, MoneyExpression.Money> CalculateTotal(Option<MoneyEvaluationContext> context)
        => moneyExpression
            => CalculateBag(moneyExpression, context)
                .CalculateTotal(context);

    private static MoneyBag CalculateBag(MoneyExpression moneyExpression, Option<MoneyEvaluationContext> context)
        => moneyExpression
            .Match(
                money: EvaluateMoney,
                moneySum: EvaluateSum(context),
                moneyProduct: EvaluateProduct(context),
                moneyDistributionPart: EvaluatePart(context));

    private static MoneyBag EvaluateMoney(MoneyExpression.Money money)
        => new(money);

    private static Func<MoneyExpression.MoneySum, MoneyBag> EvaluateSum(Option<MoneyEvaluationContext> context)
    => sum
        => CalculateBag(sum.Left, context)
            .Merge(CalculateBag(sum.Right, context));

    private static Func<MoneyExpression.MoneyProduct, MoneyBag> EvaluateProduct(Option<MoneyEvaluationContext> context)
        => product
            => CalculateBag(product.Expression, context)
                .Multiply(product.Factor);

    private static Func<MoneyExpression.MoneyDistributionPart, MoneyBag> EvaluatePart(Option<MoneyEvaluationContext> context)
        => part
            => new(CreateDistributionStrategy(context).Distribute(part, CalculateBag(part.Distribution.Expression, context).CalculateTotal(context)));

    private static Func<MoneyExpression.Money, MoneyExpression.Money> Round(Option<MoneyEvaluationContext> context)
        => money
            => money with { Amount = FindRoundingStrategy(money, context).Round(money.Amount) };

    private static IDistributionStrategy CreateDistributionStrategy(Option<MoneyEvaluationContext> context)
        => new DefaultDistributionStrategy(context);

    private static IRoundingStrategy FindRoundingStrategy(MoneyExpression.Money money, Option<MoneyEvaluationContext> context)
        => context
            .AndThen(c => c.RoundingStrategy)
            .GetOrElse(money.RoundingStrategy);
}

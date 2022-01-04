namespace Funcky;

internal interface IDistributionStrategy
{
    MoneyExpression.Money Distribute(MoneyExpression.MoneyDistributionPart part, MoneyExpression.Money total);
}

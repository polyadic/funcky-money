namespace Funcky
{
    public interface IMoneyExpression
    {
        internal void Accept(IMoneyExpressionVisitor visitor);
    }
}

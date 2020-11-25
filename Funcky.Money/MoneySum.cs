namespace Funcky
{
    internal record MoneySum : IMoneyExpression
    {
        public MoneySum(IMoneyExpression leftMoneyExpression, IMoneyExpression rightMoneyExpression)
        {
            Left = leftMoneyExpression;
            Right = rightMoneyExpression;
        }

        public IMoneyExpression Left { get; }

        public IMoneyExpression Right { get; }

        void IMoneyExpression.Accept(IMoneyExpressionVisitor visitor)
            => visitor.Visit(this);
    }
}

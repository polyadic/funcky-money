using System;
using System.Collections.Generic;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly Stack<Money> _stack = new();

        public EvaluationVisitor()
        {
        }

        public Money Result => _stack.Peek();

        public void Visit(Money money)
        {
            _stack.Push(money);
        }

        public void Visit(MoneySum sum)
        {
            sum.Left.Accept(this);
            sum.Right.Accept(this);

            var left = _stack.Pop();
            var right = _stack.Pop();

            _stack.Push(
             SameEvaluationTarget(left, right)
                ? new Money(left.Amount + right.Amount)
                : throw new MissingEvaluationContextException());
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);
            var expression = _stack.Pop();

            _stack.Push(new Money(expression.Amount * product.Factor));
        }

        public void Visit(MoneyDistributionPart part)
        {
            _stack.Push(new Money(SliceAmount(part), Option.Some(Total(part.Distribution).Currency)));
        }

        public void Visit(MoneyDistribution money)
        {
            throw new System.NotImplementedException();
        }

        private static bool SameEvaluationTarget(Money left, Money right)
            => left.Currency == right.Currency;

        private static decimal SliceAmount(MoneyDistributionPart part)
            => Ɛ(part) * part.Index < DistributionRest(part)
                ? Slice(part.Distribution, part.Index) + Ɛ(part)
                : Slice(part.Distribution, part.Index);

        private static decimal Ɛ(MoneyDistributionPart part)
            => PowerOfTen(-Total(part.Distribution).Currency.MinorUnitDigits);

        private static Money Total(MoneyDistribution distribution)
            => distribution.MoneyExpression.Evaluate();

        private static decimal DistributionRest(MoneyDistributionPart part)
            => Total(part.Distribution).Amount - DistributedTotal(part);

        private static decimal DistributedTotal(MoneyDistributionPart part)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index));

        private static decimal Slice(MoneyDistribution distribution, int index)
            => Truncate(ExactSlice(distribution, index), Total(distribution).Currency.MinorUnitDigits);

        private static decimal ExactSlice(MoneyDistribution distribution, int index)
            => Total(distribution).Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private static decimal Truncate(decimal amount, int digits)
            => decimal.Truncate(amount * PowerOfTen(digits)) / PowerOfTen(digits);

        private static decimal PowerOfTen(int exponent)
            => Enumerable.Repeat(exponent > 0 ? 10m : 0.1m, Math.Abs(exponent)).Aggregate(1m, (p, b) => b * p);

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();
    }
}

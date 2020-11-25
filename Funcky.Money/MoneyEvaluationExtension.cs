using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    public static class MoneyEvaluationExtension
    {
        public static Money Evaluate(this IMoneyExpression moneyExpression)
            => moneyExpression switch
            {
                Money money => money,
                MoneySum sum => Evaluate(sum),
                MoneyProduct product => Evaluate(product),
                MoneyDistributionPart part => Evaluate(part),
                _ => throw new NotImplementedException(),
            };

        private static Money Evaluate(MoneySum sum)
        {
            var left = sum.Left.Evaluate();
            var right = sum.Right.Evaluate();

            return SameEvaluationTarget(left, right)
                ? new Money(left.Amount + right.Amount)
                : throw new MissingEvaluationContextException();
        }

        private static Money Evaluate(MoneyProduct product)
            => new Money(product.Expression.Evaluate().Amount * product.Factor);

        private static Money Evaluate(MoneyDistributionPart part)
            => new Money(SliceAmount(part), Option.Some(Total(part.Distribution).Currency));

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

        private static bool SameEvaluationTarget(Money left, Money right)
            => left.Currency == right.Currency;
    }
}

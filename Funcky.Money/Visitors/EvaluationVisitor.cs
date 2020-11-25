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
        private readonly Option<MoneyEvaluationContext> _context;

        public EvaluationVisitor(Option<MoneyEvaluationContext> context)
        {
            _context = context;
        }

        public Money Result => _stack.Peek();

        public void Visit(Money money)
        {
            var targetCurrency = _context.AndThen(c => c.TargetCurrency);

            var result = _context.Match(
                none: money,
                some: c => ExchangeToTargetCurrency(money, c.TargetCurrency));

            _stack.Push(result);
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
            ((IMoneyExpression)part.Distribution).Accept(this);

            var partAmount = SliceAmount(part);
            var expression = _stack.Pop();

            _stack.Push(new Money(partAmount, Option.Some(expression.Currency)));
        }

        public void Visit(MoneyDistribution distribution)
            => distribution.Expression.Accept(this);

        private static bool SameEvaluationTarget(Money left, Money right)
            => left.Currency == right.Currency;

        private decimal SliceAmount(MoneyDistributionPart part)
            => Ɛ(part) * part.Index < DistributionRest(part)
                ? Slice(part.Distribution, part.Index) + Ɛ(part)
                : Slice(part.Distribution, part.Index);

        private decimal Ɛ(MoneyDistributionPart part)
            => PowerOfTen(-_stack.Peek().Currency.MinorUnitDigits);

        private decimal DistributionRest(MoneyDistributionPart part)
            => _stack.Peek().Amount - DistributedTotal(part);

        private decimal DistributedTotal(MoneyDistributionPart part)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index));

        private decimal Slice(MoneyDistribution distribution, int index)
            => Truncate(ExactSlice(distribution, index), _stack.Peek().Currency.MinorUnitDigits);

        private decimal ExactSlice(MoneyDistribution distribution, int index)
            => _stack.Peek().Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private Money ExchangeToTargetCurrency(Money money, Currency targetCurrency)
        {
            return money.Currency == targetCurrency
                ? money
                : _context.Match(
                    none: () => throw new MissingEvaluationContextException("No context"),
                    some: c => c.ExchangeRates.TryGetValue(money.Currency).Match(
                        none: () => throw new MissingEvaluationContextException($"No exchange rate from: {money.Currency.CurrencyName} to: TARGET"),
                        some: e => new Money(money.Amount * e, Option.Some(c.TargetCurrency))));
        }

        private static decimal Truncate(decimal amount, int digits)
            => decimal.Truncate(amount * PowerOfTen(digits)) / PowerOfTen(digits);

        private static decimal PowerOfTen(int exponent)
            => Enumerable.Repeat(exponent > 0 ? 10m : 0.1m, Math.Abs(exponent)).Aggregate(1m, (p, b) => b * p);

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();
    }
}

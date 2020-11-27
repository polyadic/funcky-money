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

            if (right.Amount == 0m)
            {
                _stack.Push(left);
            }
            else if (left.Amount == 0m)
            {
                _stack.Push(right);
            }
            else if (SameEvaluationTarget(left, right))
            {
                _stack.Push(left with { Amount = left.Amount + right.Amount });
            }
            else
            {
                throw new MissingEvaluationContextException();
            }
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);
            var expression = _stack.Pop();

            _stack.Push(expression with { Amount = expression.Amount * product.Factor });
        }

        public void Visit(MoneyDistributionPart part)
        {
            ((IMoneyExpression)part.Distribution).Accept(this);

            var partAmount = SliceAmount(part);
            var expression = _stack.Pop();

            _stack.Push(expression with { Amount = partAmount, Currency = expression.Currency });
        }

        public void Visit(MoneyDistribution distribution)
            => distribution.Expression.Accept(this);

        private static bool SameEvaluationTarget(Money left, Money right)
            => left.Currency == right.Currency
                && left.Precision == right.Precision
                && left.MidpointRounding == right.MidpointRounding;

        private decimal SliceAmount(MoneyDistributionPart part)
            => part.Index switch
            {
                _ when Ɛ() * (part.Index + 1) < ToDistribute(part) => Slice(part.Distribution, part.Index) + Ɛ(),
                _ when Ɛ() * part.Index < ToDistribute(part) => Slice(part.Distribution, part.Index) + ToDistribute(part) - AlreadyDistributed(part),
                _ => Slice(part.Distribution, part.Index),
            };

        private decimal AlreadyDistributed(MoneyDistributionPart part)
        {
            return Ɛ() * part.Index;
        }

        private decimal Ɛ()
            => _context.AndThen(c => c.Precision).GetOrElse(_stack.Peek().Precision);

        private decimal ToDistribute(MoneyDistributionPart part)
            => _stack.Peek().Amount - DistributedTotal(part);

        private decimal DistributedTotal(MoneyDistributionPart part)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index));

        private decimal Slice(MoneyDistribution distribution, int index)
            => Truncate(ExactSlice(distribution, index), _stack.Peek().Precision);

        private decimal ExactSlice(MoneyDistribution distribution, int index)
            => _stack.Peek().Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private Money ExchangeToTargetCurrency(Money money, Currency targetCurrency) => money.Currency == targetCurrency
                ? money
                : _context.Match(
                    none: () => throw new MissingEvaluationContextException("No context"),
                    some: c => c.ExchangeRates.TryGetValue(money.Currency).Match(
                        none: () => throw new MissingEvaluationContextException($"No exchange rate from: {money.Currency.CurrencyName} to: TARGET"),
                        some: e => money with { Amount = money.Amount * e, Currency = c.TargetCurrency }));

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();
    }
}

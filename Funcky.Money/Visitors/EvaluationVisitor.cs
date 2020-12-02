using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly Option<MoneyEvaluationContext> _context;
        private Option<Money> _result;

        public EvaluationVisitor(Option<MoneyEvaluationContext> context)
        {
            _context = context;
        }

        public Money Result
            => _result.GetOrElse(() => throw new Exception("this should not happen"));

        public void Visit(Money money)
        {
            var result = _context.Match(
                none: money,
                some: c => ExchangeToTargetCurrency(money, c.TargetCurrency));

            _result = result;
        }

        public void Visit(MoneySum sum)
        {
            sum.Left.Accept(this);
            var left = Result;

            sum.Right.Accept(this);
            var right = Result;

            if (right.Amount == 0m)
            {
                _result = left;
            }
            else if (left.Amount == 0m)
            {
                _result = right;
            }
            else if (SameEvaluationTarget(left, right))
            {
                _result = left with { Amount = left.Amount + right.Amount };
            }
            else
            {
                throw new MissingEvaluationContextException();
            }
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);

            _result = Result with { Amount = Result.Amount * product.Factor };
        }

        public void Visit(MoneyDistributionPart part)
        {
            ((IMoneyExpression)part.Distribution).Accept(this);

            var partAmount = SliceAmount(part);

            _result = Result with { Amount = partAmount, Currency = Result.Currency };
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
            => Ɛ() * part.Index;

        private decimal Ɛ()
            => _context.AndThen(c => c.Precision).GetOrElse(Result.Precision);

        private decimal ToDistribute(MoneyDistributionPart part)
            => Result.Amount - DistributedTotal(part);

        private decimal DistributedTotal(MoneyDistributionPart part)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index));

        private decimal Slice(MoneyDistribution distribution, int index)
            => Truncate(ExactSlice(distribution, index), Result.Precision);

        private decimal ExactSlice(MoneyDistribution distribution, int index)
            => Result.Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private Money ExchangeToTargetCurrency(Money money, Currency targetCurrency) => money.Currency == targetCurrency
                ? money
                : _context.Match(
                    none: () => throw new MissingEvaluationContextException("No context"),
                    some: c => money with { Amount = money.Amount * c.Bank.ExchangeRate(money.Currency, targetCurrency), Currency = c.TargetCurrency });

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();
    }
}

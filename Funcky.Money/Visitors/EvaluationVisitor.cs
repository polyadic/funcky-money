using System;
using System.Linq;
using Funcky.Extensions;
using Funcky.Monads;

namespace Funcky
{
    internal class EvaluationVisitor : IMoneyExpressionVisitor
    {
        private readonly Option<MoneyEvaluationContext> _context;

        private readonly MoneyBags _moneyBags = new();

        public EvaluationVisitor(Option<MoneyEvaluationContext> context)
        {
            _context = context;
        }

        public Money Result => Round(RawResult);

        private Money RawResult => _moneyBags.CalculateTotal(_context);

        public void Visit(Money money)
            => _moneyBags.Add(money);

        public void Visit(MoneySum sum)
        {
            sum.Left.Accept(this);
            sum.Right.Accept(this);
        }

        public void Visit(MoneyProduct product)
        {
            product.Expression.Accept(this);

            _moneyBags.Multiply(product.Factor);
        }

        public void Visit(MoneyDistributionPart part)
        {
            ((IMoneyExpression)part.Distribution).Accept(this);

            var partAmount = SliceAmount(part);

            if (ToDistribute(part) / Ɛ() == decimal.Truncate(ToDistribute(part) / Ɛ()))
            {
                throw new ImpossibleDistributionException($"It is impossible to distribute {ToDistribute(part)} in sizes of {Ɛ()}");
            }

            // we have to evaluate the money bag before we clear it
            var result = RawResult;
            _moneyBags.Clear();
            _moneyBags.Add(result with { Amount = partAmount, Currency = result.Currency });
        }

        public void Visit(MoneyDistribution distribution)
            => distribution.Expression.Accept(this);

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
            => _context.AndThen(c => c.Precision).GetOrElse(RawResult.Precision);

        private decimal ToDistribute(MoneyDistributionPart part)
            => RawResult.Amount - DistributedTotal(part);

        private decimal DistributedTotal(MoneyDistributionPart part)
            => part
                .Distribution
                .Factors
                .WithIndex()
                .Sum(f => Slice(part.Distribution, f.Index));

        private decimal Slice(MoneyDistribution distribution, int index)
            => Truncate(ExactSlice(distribution, index), RawResult.Precision);

        private decimal ExactSlice(MoneyDistribution distribution, int index)
            => RawResult.Amount / DistributionTotal(distribution) * distribution.Factors[index];

        private static decimal Truncate(decimal amount, decimal precision)
            => decimal.Truncate(amount / precision) * precision;

        private static int DistributionTotal(MoneyDistribution distribution)
            => distribution.Factors.Sum();

        private static Money Round(Money money)
            => money with { Amount = Math.Round(money.Amount / money.Precision, money.MidpointRounding) * money.Precision };
    }
}

using System;
using System.Collections.Generic;
using Funcky.Monads;

namespace Funcky
{
    internal sealed class EvaluationVisitor : IMoneyExpressionVisitor<EvaluationVisitor.State>
    {
        internal sealed record State(
            IDistributionStrategy DistributionStrategy,
            Option<MoneyEvaluationContext> Context,
            Stack<MoneyBag> MoneyBags);

        private static readonly Lazy<EvaluationVisitor> LazyInstance = new(() => new());

        public static EvaluationVisitor Instance
            => LazyInstance.Value;

        public State Visit(Money money, State state)
        {
            state.MoneyBags.Peek().Add(money);

            return state;
        }

        public State Visit(MoneySum sum, State state)
        {
            PushMoneyBag(sum.Left.Accept(this, state));
            sum.Right.Accept(this, state);

            var moneyBags = state.MoneyBags.Pop();
            state.MoneyBags.Peek().Merge(moneyBags);

            return state;
        }

        public State Visit(MoneyProduct product, State state)
        {
            product.Expression.Accept(this, state);

            state.MoneyBags.Peek().Multiply(product.Factor);

            return state;
        }

        public State Visit(MoneyDistributionPart part, State state)
        {
            part.Distribution.Expression.Accept(this, state);

            PushMoneyBag(state, state.DistributionStrategy.Distribute(part, state.MoneyBags.Pop().CalculateTotal(state.Context)));

            return state;
        }

        private void PushMoneyBag(State state, Option<Money> money = default)
        {
            var moneyBag = new MoneyBag();

            money.AndThen(m => moneyBag.Add(m));

            state.MoneyBags.Push(moneyBag);
        }
    }
}

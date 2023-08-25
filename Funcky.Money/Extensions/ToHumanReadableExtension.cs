using System.Numerics;

namespace Funcky;

public static class ToHumanReadableExtension
{
    public static string ToHumanReadable<TUnderlyingType>(this IMoneyExpression<TUnderlyingType> moneyExpression)
        where TUnderlyingType : IFloatingPoint<TUnderlyingType>
        => moneyExpression.Accept(ToHumanReadableVisitor<TUnderlyingType>.Instance);
}

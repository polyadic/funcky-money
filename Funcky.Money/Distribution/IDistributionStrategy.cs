using System.Numerics;

namespace Funcky;

internal interface IDistributionStrategy<TUnderlyingType>
    where TUnderlyingType : IFloatingPoint<TUnderlyingType>
{
    Money<TUnderlyingType> Distribute(MoneyDistributionPart<TUnderlyingType> part, Money<TUnderlyingType> total);
}

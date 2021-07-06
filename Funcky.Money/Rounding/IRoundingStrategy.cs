using System;

namespace Funcky
{
    public interface IRoundingStrategy : IEquatable<IRoundingStrategy>
    {
        decimal Round(decimal value);
    }
}

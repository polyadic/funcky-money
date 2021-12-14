using System;

namespace Funcky;

public interface IRoundingStrategy : IEquatable<IRoundingStrategy>
{
    public decimal Round(decimal value);
}

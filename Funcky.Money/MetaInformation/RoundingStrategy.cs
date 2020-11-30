using System;

namespace Funcky
{
    public static class RoundingStrategy
    {
        public const MidpointRounding BankersRounding = MidpointRounding.ToEven;
        public const MidpointRounding DefaultRoundingStrategy = BankersRounding;
    }
}

namespace Funcky.Test
{
    public record SwissMoney(Money Get)
    {
        public const decimal SmallestCoin = 0.05m;
    }
}

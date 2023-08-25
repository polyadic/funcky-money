namespace Funcky.Test;

public record SwissMoney(Money<decimal> Get)
{
    public const decimal SmallestCoin = 0.05m;
}

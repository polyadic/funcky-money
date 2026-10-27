namespace Funcky;

public sealed record Money(decimal Amount, Currency Currency)
{
    public Money Add(Money other)
        => this with { Amount = Amount + other.Amount };

    public Money Evaluate()
        => this;
}

namespace Funcky;

public sealed record Money(decimal Amount)
{
    public Money Evaluate()
        => this;
}

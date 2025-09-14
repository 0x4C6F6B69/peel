namespace Peel.Infrastructure;

public static class NumberExtensions
{
    public static decimal Increase(this decimal value, decimal percent) => value * (1M + percent / 100M);
}

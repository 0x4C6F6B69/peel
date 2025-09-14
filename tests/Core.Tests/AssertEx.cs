using Xunit.Sdk;

public static class AssertEx
{
    public static void TolerantEqual(long expected, long actual, long tolerance)
    {
        if (Math.Abs(expected - actual) > tolerance)
        {
            throw new XunitException(
                $"Expected: {expected} (±{tolerance}), but got: {actual}.");
        }
    }
}

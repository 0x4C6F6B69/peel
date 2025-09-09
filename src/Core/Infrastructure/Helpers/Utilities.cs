namespace Peel.Infrastructure;

public static class Converter
{
    public static DateTime UnixMsToDateTime(long unixMs)
    {
        const long MIN_UNIX_MS = 946_684_800_000; // begin of 2000 utc
        if (unixMs < MIN_UNIX_MS) {
            throw new ArgumentException("Unix timestamp must be on or after January 1, 2000.");
        }

        DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return epoch.AddMilliseconds(unixMs);
    }

    public static long BitcoinToSatoshi(decimal btc)
    {
        if (btc < 0) {
            throw new ArgumentOutOfRangeException(nameof(btc), $"{nameof(btc)} cannot be lesser than zero.");
        }

        const long SATS_PER_BTC = 100_000_000;

        // Round down to nearest whole satoshi
        return (long)(btc * SATS_PER_BTC);
    }

    public static decimal SatoshiToBitcoin(long sat)
    {
        if (sat < 0) {
            throw new ArgumentOutOfRangeException(nameof(sat), $"{nameof(sat)} cannot be lesser than zero.");
        }

        const long SATS_PER_BTC = 100_000_000;

        return sat / (decimal)SATS_PER_BTC;
    }

    public static long FiatToSatoshi(decimal fiatAmount, decimal btcUnitPrice)
    {
        if (fiatAmount < 0) {
            throw new ArgumentOutOfRangeException(nameof(fiatAmount), $"{nameof(fiatAmount)} cannot be lesser than zero.");
        }
        if (fiatAmount < 0) {
            throw new ArgumentOutOfRangeException(nameof(btcUnitPrice), $"{nameof(btcUnitPrice)} cannot be lesser than zero.");
        }
        
        const decimal SATS_PER_BTC = 100_000_000m;

        decimal btc = fiatAmount / btcUnitPrice;
        decimal satsDec = btc * SATS_PER_BTC;

        // floor to whole satoshis
        return (long)decimal.Floor(satsDec);
    }
}

public static class Percent
{
    public static float Increase(float value, float percent)
    {
        return value * (1f + percent / 100f);
    }

    public static float Decrease(float value, float percent)
    {
        return value * (1f - percent / 100f);
    }

    public static decimal Increase(decimal value, decimal percent)
    {
        return value * (1M + percent / 100M);
    }

    public static decimal Decrease(decimal value, decimal percent)
    {
        return value * (1M - percent / 100M);
    }
}

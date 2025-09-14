using System.Globalization;
using Peel.Configuration;
using Peel.Models;
using Microsoft.Extensions.Options;
using SharpX;
using PeachClient;

namespace Peel.Services;

public class MarketAnalyzer(PeachApiClient peachClient,
    BinanceClient binanceClient)
{
    public async Task<Maybe<(VolatilityLevel, double)>> ComputeVolatilityAsync(string fiat, float hours)
    {
        var candles = await binanceClient.GetCandlesAsync($"BTC{fiat.ToUpper()}", "1m", (int)(hours * 60));

        if (candles.Count < 2)
            return Maybe.Nothing<(VolatilityLevel, double)>();

        var absChanges = new List<double>();
        var totalAbsChange = 0.0;
        var maxAbsChange = 0.0;

        for (int i = 1; i < candles.Count; i++) {
            // Assuming candles[i] is an array/list and close price is at index 4
            var prevClose = double.Parse(candles[i - 1][4].ToString(), CultureInfo.InvariantCulture);
            var currClose = double.Parse(candles[i][4].ToString(), CultureInfo.InvariantCulture);

            // Avoid division by zero
            if (prevClose == 0) {
                continue; // Skip this interval if previous close is zero
            }

            var absChange = Math.Abs((currClose - prevClose) / prevClose);
            absChanges.Add(absChange);
            totalAbsChange += absChange;
            if (absChange > maxAbsChange) {
                maxAbsChange = absChange;
            }
        }

        if (absChanges.Count == 0) // All prevClose were 0, or no valid changes
        {
            return Maybe.Nothing<(VolatilityLevel, double)>();
        }

        // Calculate Average Absolute Percentage Change
        var averageAbsChange = totalAbsChange / absChanges.Count;

        // --- Chosen Approach: Use Average as the primary Score and basis for Level ---
        // Motivation:
        // 1. Average provides a good overall measure of typical short-term movement sensitivity
        // 2. It incorporates all the small fluctuations that contribute to general volatility
        // 3. Scaling to percentage (e.g., 0.002 -> 0.2) makes the score more interpretable
        var score = averageAbsChange * 100; // Convert to percentage (e.g., 0.002 becomes 0.2)

        // --- Determine Level based on Average Absolute Change ---
        // Thresholds should be calibrated based on observed BTC/EUR behavior
        VolatilityLevel level;
        if (averageAbsChange < 0.0001)        // < 0.01% average change per minute
            level = VolatilityLevel.VeryLow;
        else if (averageAbsChange < 0.0003)   // 0.01% - 0.03%
            level = VolatilityLevel.Low;
        else if (averageAbsChange < 0.0007)   // 0.03% - 0.07%
            level = VolatilityLevel.Medium;
        else if (averageAbsChange < 0.0015)   // 0.07% - 0.15%
            level = VolatilityLevel.High;
        else                                  // >= 0.15%
            level = VolatilityLevel.Extreme;

        // Override level if there was a very large single spike (using MaxAbsChange)
        // This adds sensitivity to sudden, significant moves even if the average is moderate.
        if (maxAbsChange >= 0.01) // If any single minute moved >= 1%
        {
            level = VolatilityLevel.Extreme;
        }

        return (level, score).ToJust();
    }

    public async Task<Maybe<decimal>> GetBtcMarketPriceAsync(string fiat) =>
        (await peachClient.GetBtcMarketPriceAsync(fiat)).Map(p => p.Price);
}

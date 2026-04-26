using System.Globalization;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Formats large approximate counts for compact UI display.
/// </summary>
public static class CompactNumberFormatter
{
    private static readonly (decimal Threshold, string Suffix)[] Units =
    [
        (1m, string.Empty),
        (1_000m, "k"),
        (1_000_000m, "M"),
        (1_000_000_000m, "B"),
        (1_000_000_000_000m, "T")
    ];

    public static string FormatCount(long value)
    {
        decimal absoluteValue = Math.Abs((decimal)value);

        if (absoluteValue < 1_000m)
            return value.ToString(CultureInfo.InvariantCulture);

        int unitIndex = 1;

        while (unitIndex + 1 < Units.Length && absoluteValue >= Units[unitIndex + 1].Threshold)
            unitIndex++;

        while (true)
        {
            (decimal threshold, string suffix) = Units[unitIndex];
            decimal roundedValue = Math.Round(value / threshold, 1, MidpointRounding.AwayFromZero);

            if (Math.Abs(roundedValue) < 1_000m || unitIndex + 1 >= Units.Length)
                return $"{roundedValue.ToString("0.#", CultureInfo.InvariantCulture)}{suffix}";

            unitIndex++;
        }
    }
}

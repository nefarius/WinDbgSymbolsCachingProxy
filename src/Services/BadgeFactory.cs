using System.Globalization;
using System.Text.RegularExpressions;

using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public interface IBadgeFactory
{
    BadgeModel GetBadge(BadgeParameters p);
}

public partial class BadgeFactory : IBadgeFactory
{
    private static readonly Regex HexColorRegex = MyRegex();

    public BadgeModel GetBadge(BadgeParameters p)
    {
        return new BadgeModel
        {
            Label = p.Label,
            Result = p.Result,
            Height = p.Height,
            LabelBackgroundColor = GetColor(p.LabelColor),
            ResultBackgroundColor = GetColor(p.ResultColor)
        };
    }

    private SKColor GetColor(string s)
    {
        Match m = HexColorRegex.Match(s);

        byte r = 0, g = 0, b = 0, a = 255;
        if (m.Success)
        {
            byte.TryParse(m.Groups["r"].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out r);
            byte.TryParse(m.Groups["g"].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out g);
            byte.TryParse(m.Groups["b"].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out b);
            byte.TryParse(m.Groups["a"].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out a);
        }

        return new SKColor(r, g, b, a);
    }

    [GeneratedRegex("#(?<r>[0-9a-f]{2})(?<g>[0-9a-f]{2})(?<b>[0-9a-f]{2})(?<a>[0-9a-f]{2})")]
    private static partial Regex MyRegex();
}
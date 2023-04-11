using SkiaSharp;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

using WinDbgSymbolsCachingProxy.Models;

namespace Badger.Factory
{
    public interface IBadgeFactory
    {
        BadgeModel GetBadge(ParameterModel p);
    }

    public class BadgeFactory : IBadgeFactory
    {
        private static Regex HexColorRegex = new Regex("#(?<r>[0-9a-f]{2})(?<g>[0-9a-f]{2})(?<b>[0-9a-f]{2})(?<a>[0-9a-f]{2})");

        private SKColor GetColor(string s)
        {
            var m = HexColorRegex.Match(s);

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

        public BadgeModel GetBadge(ParameterModel p)
        {
            return new BadgeModel
            {
                Label = p.Label,
                Result = p.Result,
                Height = p.Height,
                LabelBackgroundColor = this.GetColor(p.LabelColor),
                ResultBackgroundColor = this.GetColor(p.ResultColor)
            };
        }
    }
}

using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Service that renders badge graphics as SVG into a stream.
/// </summary>
public interface ISvgService
{
    /// <summary>
    ///     Draws a badge described by <paramref name="p"/> as SVG into <paramref name="result"/>.
    /// </summary>
    /// <param name="p">Badge parameters (label, result, colors, height).</param>
    /// <param name="result">Stream to write the SVG output to.</param>
    void Draw(BadgeParameters p, Stream result);
}

/// <summary>
///     Renders badge parameters as SVG using Skia's SVG canvas and the badge service.
/// </summary>
public class SvgService(
    IBadgeService badgeService,
    IBadgeFactory badgeFactory) : ISvgService
{
    /// <inheritdoc />
    public void Draw(BadgeParameters p, Stream result)
    {
        BadgeModel badge = badgeFactory.GetBadge(p);
        SKFont font = new(SKTypeface.FromFamilyName("Verdana"), badge.Height - (badge.TopBottomMargin * 2));
        float width = badgeService.GetWidth(badge, font);
        SKRect bounds = new(0, 0, width, p.Height);
        using SKCanvas? canvas = SKSvgCanvas.Create(bounds, result);
        badgeService.DrawBadge(canvas, badge, font);
    }
}
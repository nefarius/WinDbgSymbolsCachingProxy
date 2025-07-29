using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public interface ISvgService
{
    void Draw(BadgeParameters p, Stream result);
}

public class SvgService(
    IBadgeService badgeService,
    IBadgeFactory badgeFactory) : ISvgService
{
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
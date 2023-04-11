using Badger.Factory;

using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public interface ISvgService
{
    void Draw(ParameterModel p);
}

public class SvgService : ISvgService
{
    private readonly IBadgeFactory _badgeFactory;
    private readonly IBadgeService _badgeService;

    public SvgService(IBadgeService badgeService,
        IBadgeFactory badgeFactory)
    {
        _badgeService = badgeService;
        _badgeFactory = badgeFactory;
    }

    public void Draw(ParameterModel p)
    {
        using (SKFileWStream stream = new SKFileWStream(p.OutputFile))
        using (SKXmlStreamWriter writer = new SKXmlStreamWriter(stream))
        {
            BadgeModel badge = _badgeFactory.GetBadge(p);
            float width = _badgeService.GetWidth(badge);
            SKRect bounds = new SKRect(0, 0, width, p.Height);
            using (SKCanvas? canvas = SKSvgCanvas.Create(bounds, writer))
            {
                _badgeService.DrawBadge(canvas, badge);
            }
        }
    }
}
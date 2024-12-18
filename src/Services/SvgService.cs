﻿using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public interface ISvgService
{
    void Draw(BadgeParameters p, Stream result);
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

    public void Draw(BadgeParameters p, Stream result)
    {
        BadgeModel badge = _badgeFactory.GetBadge(p);
        SKFont font = new(SKTypeface.FromFamilyName("Verdana"), badge.Height - (badge.TopBottomMargin * 2));
        float width = _badgeService.GetWidth(badge, font);
        SKRect bounds = new(0, 0, width, p.Height);
        using SKCanvas? canvas = SKSvgCanvas.Create(bounds, result);
        _badgeService.DrawBadge(canvas, badge, font);
    }
}
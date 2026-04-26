using SkiaSharp;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Renders a 1200×630 PNG preview for Open Graph (e.g. Discord embeds).
/// </summary>
public interface IStatusOpenGraphImageRenderer
{
    /// <summary>
    ///     Writes a PNG representation of <paramref name="overview" /> to <paramref name="destination" />.
    /// </summary>
    void Render(RootResponse overview, Stream destination);
}

/// <inheritdoc />
public sealed class StatusOpenGraphImageRenderer : IStatusOpenGraphImageRenderer
{
    private const int Width = 1200;
    private const int Height = 630;

    /// <inheritdoc />
    public void Render(RootResponse overview, Stream destination)
    {
        using SKBitmap bitmap = new(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(new SKColor(0x1A, 0x1A, 0x2E));

        using SKFont titleFont = new(SKTypeface.FromFamilyName("Verdana", SKFontStyle.Bold), 52);
        using SKFont statValueFont = new(SKTypeface.FromFamilyName("Verdana", SKFontStyle.Bold), 56);
        using SKFont statLabelFont = new(SKTypeface.FromFamilyName("Verdana"), 26);
        using SKFont footerFont = new(SKTypeface.FromFamilyName("Verdana"), 20);

        using SKPaint titlePaint = new() { IsAntialias = true, Color = SKColors.White };
        using SKPaint mutedPaint = new() { IsAntialias = true, Color = new SKColor(0xAA, 0xAA, 0xBB) };
        using SKPaint footerPaint = new() { IsAntialias = true, Color = new SKColor(0x88, 0x88, 0x99) };

        float titleBaseline = 95;
        canvas.DrawText("Symbols cache status", 56, titleBaseline, titleFont, titlePaint);
        canvas.DrawText("Symbols Server", 56, titleBaseline + 52, statLabelFont, mutedPaint);

        float rowTop = 240;
        float colWidth = (Width - 56 * 2) / 3f;
        DrawStatColumn(canvas, CompactNumberFormatter.FormatCount(overview.CachedSymbolsTotal), "Cached total",
            new SKColor(0x0F, 0x82, 0xBF), 56, rowTop, colWidth, statValueFont, statLabelFont);
        DrawStatColumn(canvas, CompactNumberFormatter.FormatCount(overview.CachedSymbolsFound), "Found",
            new SKColor(0x2A, 0xC3, 0x3B), 56 + colWidth, rowTop, colWidth, statValueFont, statLabelFont);
        DrawStatColumn(canvas, CompactNumberFormatter.FormatCount(overview.CachedSymbols404), "Not found (404)",
            new SKColor(0xE6, 0xA2, 0x3C), 56 + colWidth * 2, rowTop, colWidth, statValueFont, statLabelFont);

        string footer = overview.ServerVersion is { Length: > 0 } v
            ? $"Version {v} · {overview.ProjectUrl}"
            : overview.ProjectUrl;
        canvas.DrawText(footer, 56, Height - 36, footerFont, footerPaint);

        if (!bitmap.Encode(destination, SKEncodedImageFormat.Png, 100))
            throw new InvalidOperationException("Failed to encode Open Graph status image as PNG.");
    }

    private static void DrawStatColumn(SKCanvas canvas, string value, string label, SKColor accent, float x, float top,
        float columnWidth, SKFont valueFont, SKFont labelFont)
    {
        using SKPaint valuePaint = new() { IsAntialias = true, Color = SKColors.White };
        using SKPaint labelPaint = new() { IsAntialias = true, Color = accent };

        float valueWidth = valueFont.MeasureText(value);
        float vx = x + (columnWidth - valueWidth) / 2f;
        canvas.DrawText(value, vx, top + valueFont.Size, valueFont, valuePaint);

        float labelWidth = labelFont.MeasureText(label);
        float lx = x + (columnWidth - labelWidth) / 2f;
        canvas.DrawText(label, lx, top + valueFont.Size + 44, labelFont, labelPaint);
    }
}

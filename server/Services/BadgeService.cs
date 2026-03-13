using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Service for measuring and drawing badge graphics onto an Skia canvas.
/// </summary>
public interface IBadgeService
{
    /// <summary>
    ///     Draws the badge (label + result) onto the given canvas using the specified font.
    /// </summary>
    /// <param name="canvas">The Skia canvas to draw on.</param>
    /// <param name="badge">The badge model (label, result, colors, dimensions).</param>
    /// <param name="font">The font to use for text.</param>
    void DrawBadge(SKCanvas canvas, BadgeModel badge, SKFont font);

    /// <summary>
    ///     Returns the total width in pixels required to draw the badge.
    /// </summary>
    /// <param name="badge">The badge model.</param>
    /// <param name="font">The font used for measuring text.</param>
    /// <returns>Width in pixels.</returns>
    public float GetWidth(BadgeModel badge, SKFont font);
}

/// <summary>
///     Renders badge graphics (label and result with background colors) using SkiaSharp.
/// </summary>
public class BadgeService : IBadgeService
{
    /// <inheritdoc />
    public float GetWidth(BadgeModel badge, SKFont font)
    {
        int outerMargin = GetOuterMargin(badge),
            innerMargin = GetInnerMargin(badge);

        float width = outerMargin
                      + font.MeasureText(badge.Label)
                      + innerMargin
                      + innerMargin
                      + font.MeasureText(badge.Result)
                      + outerMargin;

        return width;
    }

    /// <inheritdoc />
    public void DrawBadge(SKCanvas canvas, BadgeModel badge, SKFont font)
    {
        canvas.Clear();
        
        SKPaint textPaint = GetTextPaint(badge);
        float outerMargin = GetOuterMargin(badge),
            innerMargin = GetInnerMargin(badge);
        float leftTextWidth = font.MeasureText(badge.Label);
        float leftSideWidth = outerMargin + leftTextWidth + innerMargin;
        float topBottomMargin = GetTopBottomMargin(badge);
        float textY = (badge.Height / 2.0f) + font.Metrics.Bottom + 2;
        float cornerRadius = GetCornerRadius(badge);

        /* draw left background */
        SKPaint labelBackgroundPaint = GetLabelBackgroundPaint(badge);
        canvas.DrawRoundRect(0, 0, leftSideWidth, badge.Height, cornerRadius, cornerRadius, labelBackgroundPaint);
        canvas.DrawRect(leftSideWidth - cornerRadius, 0, cornerRadius, badge.Height, labelBackgroundPaint);

        /* draw right background */
        SKPaint resultBackgroundPaint = GetResultBackgroundPaint(badge);
        canvas.DrawRoundRect(leftSideWidth, 0, innerMargin + font.MeasureText(badge.Result) + outerMargin,
            badge.Height, cornerRadius, cornerRadius, resultBackgroundPaint);
        canvas.DrawRect(leftSideWidth, 0, cornerRadius, badge.Height, resultBackgroundPaint);

        /* write left text */
        SKPaint textShadowPaint = GetTextShadowPaint(badge);
        float shadowFactor = 1.05f;
        canvas.DrawText(badge.Label, outerMargin, textY * shadowFactor, font, textShadowPaint);
        canvas.DrawText(badge.Label, outerMargin, textY, font, textPaint);

        /* write right text */
        float rightTextX = leftSideWidth + innerMargin;
        canvas.DrawText(badge.Result, rightTextX, textY * shadowFactor, font, textShadowPaint);
        canvas.DrawText(badge.Result, rightTextX, textY, font, textPaint);
    }

    private static SKPaint GetTextPaint(BadgeModel badge)
    {
        return new SKPaint { IsAntialias = true, IsStroke = false, Color = SKColors.White };
    }

    private SKPaint GetTextShadowPaint(BadgeModel badge)
    {
        SKPaint paint = GetTextPaint(badge);
        paint.Color = new SKColor(0x33, 0x33, 0x33, 0xff);
        return paint;
    }

    private static SKPaint GetResultBackgroundPaint(BadgeModel badge)
    {
        return new SKPaint { IsAntialias = true, IsStroke = false, Color = badge.ResultBackgroundColor };
    }

    private static SKPaint GetLabelBackgroundPaint(BadgeModel badge)
    {
        return new SKPaint { IsAntialias = true, IsStroke = false, Color = badge.LabelBackgroundColor };
    }

    private static int GetOuterMargin(BadgeModel badge)
    {
        return badge.Height / 2;
    }

    private static int GetInnerMargin(BadgeModel badge)
    {
        return badge.Height / 4;
    }

    private static int GetTopBottomMargin(BadgeModel badge)
    {
        return badge.Height / 5;
    }

    private static int GetCornerRadius(BadgeModel badge)
    {
        return badge.Height / 5;
    }
}
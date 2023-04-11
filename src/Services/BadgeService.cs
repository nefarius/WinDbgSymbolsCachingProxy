using SkiaSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

public interface IBadgeService
{
    void DrawBadge(SKCanvas canvas, BadgeModel badge);

    float GetWidth(BadgeModel badge);
}

public class BadgeService : IBadgeService
{
    public float GetWidth(BadgeModel badge)
    {
        SKPaint paint = GetTextPaint(badge);

        int outerMargin = GetOuterMargin(badge),
            innerMargin = GetInnerMargin(badge);

        float width = outerMargin
                      + paint.MeasureText(badge.Label)
                      + innerMargin
                      + innerMargin
                      + paint.MeasureText(badge.Result)
                      + outerMargin;

        return width;
    }

    public void DrawBadge(SKCanvas canvas, BadgeModel badge)
    {
        canvas.Clear();

        SKPaint textPaint = GetTextPaint(badge);
        float outerMargin = GetOuterMargin(badge),
            innerMargin = GetInnerMargin(badge);
        float leftTextWidth = textPaint.MeasureText(badge.Label);
        float leftSideWidth = outerMargin + leftTextWidth + innerMargin;
        float topBottomMargin = GetTopBottomMargin(badge);
        float textY = (badge.Height / 2) + textPaint.FontMetrics.Bottom;
        float cornerRadius = GetCornerRadius(badge);

        /* draw left background */
        SKPaint labelBackgroundPaint = GetLabelBackgroundPaint(badge);
        canvas.DrawRoundRect(0, 0, leftSideWidth, badge.Height, cornerRadius, cornerRadius, labelBackgroundPaint);
        canvas.DrawRect(leftSideWidth - cornerRadius, 0, cornerRadius, badge.Height, labelBackgroundPaint);

        /* draw right background */
        SKPaint resultBackgroundPaint = GetResultBackgroundPaint(badge);
        canvas.DrawRoundRect(leftSideWidth, 0, innerMargin + textPaint.MeasureText(badge.Result) + outerMargin,
            badge.Height, cornerRadius, cornerRadius, resultBackgroundPaint);
        canvas.DrawRect(leftSideWidth, 0, cornerRadius, badge.Height, resultBackgroundPaint);

        /* write left text */
        SKPaint textShadowPaint = GetTextShadowPaint(badge);
        float shadowFactor = 1.05f;
        canvas.DrawText(badge.Label, outerMargin, textY * shadowFactor, textShadowPaint);
        canvas.DrawText(badge.Label, outerMargin, textY, textPaint);

        /* write right text */
        float rightTextX = leftSideWidth + innerMargin;
        canvas.DrawText(badge.Result, rightTextX, textY * shadowFactor, textShadowPaint);
        canvas.DrawText(badge.Result, rightTextX, textY, textPaint);
    }

    private static SKPaint GetTextPaint(BadgeModel badge)
    {
        return new SKPaint
        {
            IsAntialias = true,
            IsStroke = false,
            Color = SKColors.White,
            TextSize = badge.Height - (GetTopBottomMargin(badge) * 2)
        };
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
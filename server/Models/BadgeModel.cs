using SkiaSharp;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Model used by the badge rendering pipeline: label, result, dimensions and colors.
/// </summary>
public class BadgeModel
{
    /// <summary>Gets the left-hand label text.</summary>
    public string Label { get; init; }

    /// <summary>Gets the right-hand result text.</summary>
    public string Result { get; init; }

    /// <summary>Gets the badge height in pixels.</summary>
    public int Height { get; init; }

    /// <summary>Gets the background color for the result side.</summary>
    public SKColor ResultBackgroundColor { get; init; }

    /// <summary>Gets the background color for the label side.</summary>
    public SKColor LabelBackgroundColor { get; init; }

    /// <summary>Gets the top/bottom margin derived from height (height / 5).</summary>
    public int TopBottomMargin => Height / 5;
}
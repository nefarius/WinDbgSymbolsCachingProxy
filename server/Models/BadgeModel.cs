using SkiaSharp;

namespace WinDbgSymbolsCachingProxy.Models;

public class BadgeModel
{
    public string Label { get; init; }
    public string Result { get; init; }
    public int Height { get; init; }
    public SKColor ResultBackgroundColor { get; init; }
    public SKColor LabelBackgroundColor { get; init; }

    public int TopBottomMargin => Height / 5;
}
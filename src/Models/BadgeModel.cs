using SkiaSharp;

namespace WinDbgSymbolsCachingProxy.Models;

public class BadgeModel
{
    public string Label { get; set; }
    public string Result { get; set; }
    public int Height { get; set; }
    public SKColor ResultBackgroundColor { get; set; }
    public SKColor LabelBackgroundColor { get; set; }
}
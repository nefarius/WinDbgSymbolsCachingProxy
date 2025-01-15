namespace WinDbgSymbolsCachingProxy.Models;

public class BadgeParameters
{
    public BadgeParameters()
    {
        Height = 22;
        LabelColor = "#333333ff";
        ResultColor = "#00ff00ff";
    }

    /// <summary>
    ///     The left hand label/text of the badge.
    /// </summary>
    public string Label { get; set; }
    
    /// <summary>
    ///     The right hand label/value of the badge.
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    ///     Right hand side color as hex string.
    /// </summary>
    public string ResultColor { get; set; }
    
    /// <summary>
    ///     Left hand side color as hex string.
    /// </summary>
    public string LabelColor { get; set; }

    /// <summary>
    ///     Badge height in pixels.
    /// </summary>
    public int Height { get; set; }
}
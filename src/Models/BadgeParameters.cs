namespace WinDbgSymbolsCachingProxy.Models;

public class BadgeParameters
{
    public BadgeParameters()
    {
        Height = 22;
        LabelColor = "#333333ff";
        ResultColor = "#00ff00ff";
    }

    public string Label { get; set; }
    
    public string Result { get; set; }
    
    public string ResultColor { get; set; }
    
    public string LabelColor { get; set; }

    public int Height { get; set; }
}
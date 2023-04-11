namespace WinDbgSymbolsCachingProxy.Models;

public enum OutputFileFormat { Svg, Png }

public enum ActionType { ShowHelp, CreateImage }

public class ParameterModel
{
    public ParameterModel()
    {
        OutputFile = "out.svg";
        OutputFileFormat = OutputFileFormat.Svg;
        Action = ActionType.ShowHelp;
        Height = 22;
        LabelColor = "#333333ff";
        ResultColor = "#00ff00ff";
    }

    public ActionType Action { get; set; }

    public string Label { get; set; }
    public string Result { get; set; }
    public string ResultColor { get; set; }
    public string LabelColor { get; set; }

    public int Height { get; set; }

    public string OutputFile { get; set; }
    public OutputFileFormat OutputFileFormat { get; set; }

    public string HelpText { get; set; }
}
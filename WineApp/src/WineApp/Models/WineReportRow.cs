namespace WineApp.Models;

public class WineReportRow
{
    public string RatingName    { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Group         { get; set; } = string.Empty;
    public string Class         { get; set; } = string.Empty;
    public string Category      { get; set; } = string.Empty;
    public string ProducerName  { get; set; } = string.Empty;
    public double AvgAppearance { get; set; }
    public double AvgNose       { get; set; }
    public double AvgTaste      { get; set; }
    public double Total         { get; set; }
    public int    RatingCount   { get; set; }
}

namespace WineApp.Models;

public class JudgePattern
{
    public string JudgeName { get; set; } = string.Empty;
    public decimal AverageScore { get; set; }
    public bool HasLowVariance { get; set; }
    public decimal DefectFlagRate { get; set; }
    public bool TendsLow { get; set; }
    public bool TendsHigh { get; set; }
}

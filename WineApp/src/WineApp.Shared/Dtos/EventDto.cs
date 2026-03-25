namespace WineApp.Shared.Dtos;

public class EventDto
{
    public string EventId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal GoldThreshold { get; set; } = 17.0m;
    public decimal SilverThreshold { get; set; } = 15.5m;
    public decimal BronzeThreshold { get; set; } = 14.0m;
    public decimal SpecialMeritThreshold { get; set; } = 12.0m;
    public decimal AppearanceGateValue { get; set; } = 1.8m;
    public decimal NoseGateValue { get; set; } = 1.8m;
    public decimal TasteGateValue { get; set; } = 5.8m;
    public bool IsActive { get; set; }
}

namespace WineApp.Shared.Dtos;

public class WineRatingDto
{
    public string? WineRatingId { get; set; }
    public decimal Appearance { get; set; }
    public decimal Nose { get; set; }
    public decimal Taste { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string JudgeId { get; set; } = string.Empty;
    public string WineId { get; set; } = string.Empty;
    public decimal Total => Math.Round(Appearance + Nose + Taste, 1);
}

namespace WineApp.Shared.Dtos;

public class WineDto
{
    public string WineId { get; set; } = string.Empty;
    public int? WineNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RatingName { get; set; } = string.Empty;
    public int Vintage { get; set; }
    public decimal AlcoholPercentage { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string WineProducerId { get; set; } = string.Empty;
    public string? EventId { get; set; }
    public bool IsPaid { get; set; }
}

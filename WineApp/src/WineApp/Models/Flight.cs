using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineApp.Models;

/// <summary>
/// Represents a flight of wines for tasting
/// </summary>
[BsonIgnoreExtraElements]
public class Flight
{
    public List<string> AssignedJudgeIds { get; set; } = [];
    public int SecurityRevision { get; set; }
    public bool IsDeleted { get; set; }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FlightId { get; set; } = ObjectId.GenerateNewId().ToString();
    public string EventId { get; set; } = string.Empty;
    public string FlightName { get; set; } = string.Empty;
    public int FlightNumber { get; set; }
    public List<string> WineIds { get; set; } = [];
    public WineCategory? Category { get; set; }
    public WineGroup? Group { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

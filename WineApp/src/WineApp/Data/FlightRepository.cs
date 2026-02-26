using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class FlightRepository : IFlightRepository
{
    private readonly IMongoCollection<Flight> _collection;

    public FlightRepository(WineMongoDbContext context) =>
        _collection = context.Flights;

    public List<Flight> GetFlightsForEvent(string eventId) =>
        _collection.Find(f => f.EventId == eventId)
                   .SortBy(f => f.FlightNumber)
                   .ToList();

    public Flight? GetFlightById(string flightId) =>
        _collection.Find(f => f.FlightId == flightId).FirstOrDefault();

    public void AddFlight(Flight flight) =>
        _collection.InsertOne(flight);

    public void UpdateFlight(Flight flight) =>
        _collection.ReplaceOne(f => f.FlightId == flight.FlightId, flight);

    public void DeleteFlight(string flightId) =>
        _collection.DeleteOne(f => f.FlightId == flightId);
}

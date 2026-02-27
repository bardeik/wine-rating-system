using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class FlightRepository : IFlightRepository
{
    private readonly IMongoCollection<Flight> _collection;

    public FlightRepository(WineMongoDbContext context) =>
        _collection = context.Flights;

    public async Task<List<Flight>> GetFlightsForEventAsync(string eventId) =>
        await _collection.Find(f => f.EventId == eventId)
                         .SortBy(f => f.FlightNumber)
                         .ToListAsync();

    public async Task<Flight?> GetFlightByIdAsync(string flightId) =>
        await _collection.Find(f => f.FlightId == flightId).FirstOrDefaultAsync();

    public async Task AddFlightAsync(Flight flight) =>
        await _collection.InsertOneAsync(flight);

    public async Task UpdateFlightAsync(Flight flight) =>
        await _collection.ReplaceOneAsync(f => f.FlightId == flight.FlightId, flight);

    public async Task DeleteFlightAsync(string flightId) =>
        await _collection.DeleteOneAsync(f => f.FlightId == flightId);
}

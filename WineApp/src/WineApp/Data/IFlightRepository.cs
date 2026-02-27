using WineApp.Models;

namespace WineApp.Data;

public interface IFlightRepository
{
    Task<List<Flight>> GetFlightsForEventAsync(string eventId);
    Task<Flight?> GetFlightByIdAsync(string flightId);
    Task AddFlightAsync(Flight flight);
    Task UpdateFlightAsync(Flight flight);
    Task DeleteFlightAsync(string flightId);
}

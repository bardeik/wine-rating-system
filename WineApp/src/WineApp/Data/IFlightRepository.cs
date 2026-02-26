using WineApp.Models;

namespace WineApp.Data;

public interface IFlightRepository
{
    List<Flight> GetFlightsForEvent(string eventId);
    Flight? GetFlightById(string flightId);
    void AddFlight(Flight flight);
    void UpdateFlight(Flight flight);
    void DeleteFlight(string flightId);
}

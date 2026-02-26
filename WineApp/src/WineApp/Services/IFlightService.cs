using WineApp.Models;

namespace WineApp.Services;

public interface IFlightService
{
    /// <summary>
    /// Organizes wines into flights of approximately 6 wines each
    /// </summary>
    List<Flight> OrganizeFlights(string eventId, int winesPerFlight = 6);
    
    /// <summary>
    /// Gets all flights for an event
    /// </summary>
    List<Flight> GetFlightsForEvent(string eventId);
    
    /// <summary>
    /// Creates a custom flight
    /// </summary>
    Flight CreateFlight(string eventId, string flightName, List<string> wineIds);
    
    /// <summary>
    /// Updates flight order or wine assignments
    /// </summary>
    void UpdateFlight(Flight flight);
    
    /// <summary>
    /// Deletes a flight
    /// </summary>
    void DeleteFlight(string flightId);
    
    /// <summary>
    /// Gets wines in a specific flight
    /// </summary>
    List<Wine> GetWinesInFlight(string flightId);
    
    /// <summary>
    /// Auto-organize wines by category and group for optimal tasting
    /// </summary>
    List<Flight> AutoOrganizeFlights(string eventId);
}

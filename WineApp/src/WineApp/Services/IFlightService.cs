using WineApp.Models;

namespace WineApp.Services;

public interface IFlightService
{
    /// <summary>
    /// Organizes wines into flights of approximately 6 wines each
    /// </summary>
    Task<List<Flight>> OrganizeFlightsAsync(string eventId, int winesPerFlight = 6);
    
    /// <summary>
    /// Gets all flights for an event
    /// </summary>
    Task<List<Flight>> GetFlightsForEventAsync(string eventId);
    
    /// <summary>
    /// Creates a custom flight
    /// </summary>
    Task<Flight> CreateFlightAsync(string eventId, string flightName, List<string> wineIds);
    
    /// <summary>
    /// Updates flight order or wine assignments
    /// </summary>
    Task UpdateFlightAsync(Flight flight);
    
    /// <summary>
    /// Deletes a flight
    /// </summary>
    Task DeleteFlightAsync(string flightId);
    
    /// <summary>
    /// Gets wines in a specific flight
    /// </summary>
    Task<List<Wine>> GetWinesInFlightAsync(string flightId);
    
    /// <summary>
    /// Auto-organize wines by category and group for optimal tasting
    /// </summary>
    Task<List<Flight>> AutoOrganizeFlightsAsync(string eventId);
}

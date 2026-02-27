using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class FlightService : IFlightService
{
    private readonly IWineRepository _wineRepository;
    private readonly IFlightRepository _flightRepository;

    public FlightService(IWineRepository wineRepository, IFlightRepository flightRepository)
    {
        _wineRepository = wineRepository;
        _flightRepository = flightRepository;
    }

    public async Task<List<Flight>> OrganizeFlightsAsync(string eventId, int winesPerFlight = 6)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in await _flightRepository.GetFlightsForEventAsync(eventId))
            await _flightRepository.DeleteFlightAsync(existing.FlightId);

        var flightNumber = 1;
        var created = new List<Flight>();
        for (int i = 0; i < wines.Count; i += winesPerFlight)
        {
            var flight = new Flight
            {
                EventId = eventId,
                FlightNumber = flightNumber,
                FlightName = $"Flight {flightNumber}",
                WineIds = wines.Skip(i).Take(winesPerFlight).Select(w => w.WineId).ToList()
            };
            await _flightRepository.AddFlightAsync(flight);
            created.Add(flight);
            flightNumber++;
        }

        return created;
    }

    public async Task<List<Flight>> AutoOrganizeFlightsAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Group)
            .ThenBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in await _flightRepository.GetFlightsForEventAsync(eventId))
            await _flightRepository.DeleteFlightAsync(existing.FlightId);

        var flightNumber = 1;
        var created = new List<Flight>();

        foreach (var group in wines.GroupBy(w => new { w.Category, w.Group }))
        {
            for (int i = 0; i < group.Count(); i += 6)
            {
                var flight = new Flight
                {
                    EventId = eventId,
                    FlightNumber = flightNumber,
                    FlightName = $"Flight {flightNumber} - {group.Key.Category} ({group.Key.Group})",
                    WineIds = group.Skip(i).Take(6).Select(w => w.WineId).ToList(),
                    Category = group.Key.Category,
                    Group = group.Key.Group
                };
                await _flightRepository.AddFlightAsync(flight);
                created.Add(flight);
                flightNumber++;
            }
        }

        return created;
    }

    public Task<List<Flight>> GetFlightsForEventAsync(string eventId) =>
        _flightRepository.GetFlightsForEventAsync(eventId);

    public async Task<Flight> CreateFlightAsync(string eventId, string flightName, List<string> wineIds)
    {
        var existingFlights = await _flightRepository.GetFlightsForEventAsync(eventId);
        var maxFlightNumber = existingFlights
            .Select(f => f.FlightNumber)
            .DefaultIfEmpty(0)
            .Max();

        var flight = new Flight
        {
            EventId = eventId,
            FlightName = flightName,
            FlightNumber = maxFlightNumber + 1,
            WineIds = wineIds
        };
        await _flightRepository.AddFlightAsync(flight);
        return flight;
    }

    public Task UpdateFlightAsync(Flight flight) => _flightRepository.UpdateFlightAsync(flight);

    public Task DeleteFlightAsync(string flightId) => _flightRepository.DeleteFlightAsync(flightId);

    public async Task<List<Wine>> GetWinesInFlightAsync(string flightId)
    {
        var flight = await _flightRepository.GetFlightByIdAsync(flightId);
        if (flight == null) return new List<Wine>();

        var allWines = await _wineRepository.GetAllWinesAsync();
        return flight.WineIds
            .Select(wineId => allWines.FirstOrDefault(w => w.WineId == wineId))
            .Where(w => w != null)
            .Cast<Wine>()
            .ToList();
    }
}

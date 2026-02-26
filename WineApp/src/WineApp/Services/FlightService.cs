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

    public List<Flight> OrganizeFlights(string eventId, int winesPerFlight = 6)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in _flightRepository.GetFlightsForEvent(eventId))
            _flightRepository.DeleteFlight(existing.FlightId);

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
            _flightRepository.AddFlight(flight);
            created.Add(flight);
            flightNumber++;
        }

        return created;
    }

    public List<Flight> AutoOrganizeFlights(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Group)
            .ThenBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in _flightRepository.GetFlightsForEvent(eventId))
            _flightRepository.DeleteFlight(existing.FlightId);

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
                _flightRepository.AddFlight(flight);
                created.Add(flight);
                flightNumber++;
            }
        }

        return created;
    }

    public List<Flight> GetFlightsForEvent(string eventId) =>
        _flightRepository.GetFlightsForEvent(eventId);

    public Flight CreateFlight(string eventId, string flightName, List<string> wineIds)
    {
        var maxFlightNumber = _flightRepository.GetFlightsForEvent(eventId)
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
        _flightRepository.AddFlight(flight);
        return flight;
    }

    public void UpdateFlight(Flight flight) => _flightRepository.UpdateFlight(flight);

    public void DeleteFlight(string flightId) => _flightRepository.DeleteFlight(flightId);

    public List<Wine> GetWinesInFlight(string flightId)
    {
        var flight = _flightRepository.GetFlightById(flightId);
        if (flight == null) return new List<Wine>();

        var allWines = _wineRepository.GetAllWines();
        return flight.WineIds
            .Select(wineId => allWines.FirstOrDefault(w => w.WineId == wineId))
            .Where(w => w != null)
            .Cast<Wine>()
            .ToList();
    }
}

using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class FlightService : IFlightService
{
    private readonly IWineRepository _wineRepository;
    private readonly List<Flight> _flights = new(); // In-memory for now, could be moved to MongoDB

    public FlightService(IWineRepository wineRepository)
    {
        _wineRepository = wineRepository;
    }

    public List<Flight> OrganizeFlights(string eventId, int winesPerFlight = 6)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.WineNumber)
            .ToList();

        _flights.RemoveAll(f => f.EventId == eventId);

        var flightNumber = 1;
        for (int i = 0; i < wines.Count; i += winesPerFlight)
        {
            var flightWines = wines.Skip(i).Take(winesPerFlight).ToList();
            
            var flight = new Flight
            {
                EventId = eventId,
                FlightNumber = flightNumber,
                FlightName = $"Flight {flightNumber}",
                WineIds = flightWines.Select(w => w.WineId).ToList()
            };

            _flights.Add(flight);
            flightNumber++;
        }

        return _flights.Where(f => f.EventId == eventId).ToList();
    }

    public List<Flight> AutoOrganizeFlights(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Group)
            .ThenBy(w => w.WineNumber)
            .ToList();

        _flights.RemoveAll(f => f.EventId == eventId);

        var categorizedGroups = wines.GroupBy(w => new { w.Category, w.Group });
        var flightNumber = 1;

        foreach (var group in categorizedGroups)
        {
            var groupWines = group.ToList();
            
            for (int i = 0; i < groupWines.Count; i += 6)
            {
                var flightWines = groupWines.Skip(i).Take(6).ToList();
                
                var flight = new Flight
                {
                    EventId = eventId,
                    FlightNumber = flightNumber,
                    FlightName = $"Flight {flightNumber} - {group.Key.Category} ({group.Key.Group})",
                    WineIds = flightWines.Select(w => w.WineId).ToList(),
                    Category = group.Key.Category,
                    Group = group.Key.Group
                };

                _flights.Add(flight);
                flightNumber++;
            }
        }

        return _flights.Where(f => f.EventId == eventId).ToList();
    }

    public List<Flight> GetFlightsForEvent(string eventId)
    {
        return _flights.Where(f => f.EventId == eventId).OrderBy(f => f.FlightNumber).ToList();
    }

    public Flight CreateFlight(string eventId, string flightName, List<string> wineIds)
    {
        var maxFlightNumber = _flights
            .Where(f => f.EventId == eventId)
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

        _flights.Add(flight);
        return flight;
    }

    public void UpdateFlight(Flight flight)
    {
        var existing = _flights.FirstOrDefault(f => f.FlightId == flight.FlightId);
        if (existing != null)
        {
            existing.FlightName = flight.FlightName;
            existing.WineIds = flight.WineIds;
            existing.Category = flight.Category;
            existing.Group = flight.Group;
        }
    }

    public void DeleteFlight(string flightId)
    {
        _flights.RemoveAll(f => f.FlightId == flightId);
    }

    public List<Wine> GetWinesInFlight(string flightId)
    {
        var flight = _flights.FirstOrDefault(f => f.FlightId == flightId);
        if (flight == null) return new List<Wine>();

        var allWines = _wineRepository.GetAllWines();
        return flight.WineIds
            .Select(wineId => allWines.FirstOrDefault(w => w.WineId == wineId))
            .Where(w => w != null)
            .Cast<Wine>()
            .ToList();
    }
}

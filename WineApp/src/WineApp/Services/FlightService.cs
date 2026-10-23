using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class FlightService : IFlightService
{
    private readonly IWineRepository _wineRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly SecurityAccess _access;
    private readonly IEventRepository _events;
    private readonly ISecurityMutationStore _mutations;

    public FlightService(IWineRepository wineRepository, IFlightRepository flightRepository,
        SecurityAccess access, IEventRepository events, ISecurityMutationStore mutations)
    {
        _wineRepository = wineRepository;
        _flightRepository = flightRepository;
        _access = access;
        _events = events;
        _mutations = mutations;
    }

    public async Task<List<Flight>> OrganizeFlightsAsync(string eventId, int winesPerFlight = 6)
    {
        await _access.RequireAdminAsync();
        if (winesPerFlight is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(winesPerFlight));
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in await _flightRepository.GetFlightsForEventAsync(eventId))
            await DeleteFlightAsync(existing.FlightId);

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
            await SaveFlightAsync(flight, null);
            created.Add(flight);
            flightNumber++;
        }

        return created;
    }

    public async Task<List<Flight>> AutoOrganizeFlightsAsync(string eventId)
    {
        await _access.RequireAdminAsync();
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.IsPaid && w.WineNumber.HasValue)
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Group)
            .ThenBy(w => w.WineNumber)
            .ToList();

        foreach (var existing in await _flightRepository.GetFlightsForEventAsync(eventId))
            await DeleteFlightAsync(existing.FlightId);

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
                await SaveFlightAsync(flight, null);
                created.Add(flight);
                flightNumber++;
            }
        }

        return created;
    }

    public async Task<List<Flight>> GetFlightsForEventAsync(string eventId)
    {
        var user = await _access.GetUserAsync();
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin"))
            return await _flightRepository.GetFlightsForEventAsync(eventId);
        var judgeId = await _access.RequireJudgeAsync();
        if ((await _events.GetActiveEventAsync())?.EventId != eventId) return [];
        return (await _flightRepository.GetFlightsForEventAsync(eventId))
            .Where(f => f.AssignedJudgeIds.Contains(judgeId)).ToList();
    }

    public async Task<Flight> CreateFlightAsync(string eventId, string flightName, List<string> wineIds)
    {
        await _access.RequireAdminAsync();
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
        await SaveFlightAsync(flight, null);
        return flight;
    }

    public async Task UpdateFlightAsync(Flight flight)
    {
        await _access.RequireAdminAsync();
        var before = await _flightRepository.GetFlightByIdAsync(flight.FlightId)
            ?? throw new InvalidOperationException("Flight not found.");
        if (before.SecurityRevision != flight.SecurityRevision || before.EventId != flight.EventId)
            throw new InvalidOperationException("Reload the flight before editing.");
        flight.IsDeleted = false;
        await SaveFlightAsync(flight, before);
    }

    public async Task DeleteFlightAsync(string flightId)
    {
        var actor = await _access.RequireAdminAsync();
        var before = await _flightRepository.GetFlightByIdAsync(flightId)
            ?? throw new InvalidOperationException("Flight not found.");
        var after = System.Text.Json.JsonSerializer.Deserialize<Flight>(System.Text.Json.JsonSerializer.Serialize(before))!;
        after.IsDeleted = true;
        await _mutations.CommitAsync(WineMongoDbContext.FlightsCollection, flightId,
            before.SecurityRevision, before, after, actor, "FlightDeleted");
    }

    private async Task SaveFlightAsync(Flight flight, Flight? before)
    {
        var actor = await _access.RequireAdminAsync();
        await _mutations.CommitAsync(WineMongoDbContext.FlightsCollection, flight.FlightId,
            before?.SecurityRevision, before, flight, actor, before is null ? "FlightCreated" : "FlightUpdated");
        flight.SecurityRevision = (before?.SecurityRevision ?? 0) + 1;
    }

    public async Task<List<Wine>> GetWinesInFlightAsync(string flightId)
    {
        var flight = await _flightRepository.GetFlightByIdAsync(flightId);
        if (flight == null) return new List<Wine>();

        var user = await _access.GetUserAsync();
        var isAdmin = user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
        if (!isAdmin)
        {
            var judgeId = await _access.RequireJudgeAsync();
            if ((await _events.GetActiveEventAsync())?.EventId != flight.EventId || !flight.AssignedJudgeIds.Contains(judgeId))
                throw new UnauthorizedAccessException("This flight is not assigned to you.");
        }

        var allWines = await _wineRepository.GetAllWinesAsync();
        return flight.WineIds
            .Select(wineId => allWines.FirstOrDefault(w => w.WineId == wineId))
            .Where(w => w != null && (isAdmin || (w.IsPaid && w.WineNumber.HasValue && w.EventId == flight.EventId)))
            .Cast<Wine>()
            .Select(w => isAdmin ? w : new Wine
            {
                WineId = w.WineId, RatingName = w.RatingName, WineNumber = w.WineNumber,
                Category = w.Category, Group = w.Group, Class = w.Class
            })
            .ToList();
    }
}

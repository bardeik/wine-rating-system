using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class WineNumberService : IWineNumberService
{
    private readonly IWineRepository _wineRepository;

    public WineNumberService(IWineRepository wineRepository)
    {
        _wineRepository = wineRepository;
    }

    public Task<Dictionary<string, int>> AssignWineNumbersAsync(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.IsPaid)
            .ToList();

        if (!wines.Any())
            return Task.FromResult(new Dictionary<string, int>());

        // Get category order
        var categoryOrder = GetCategoryOrder();

        // Sort wines by category order
        var sortedWines = wines
            .OrderBy(w => categoryOrder.IndexOf(w.Category))
            .ThenBy(w => w.WineId) // Secondary sort for consistency
            .ToList();

        var assignments = new Dictionary<string, int>();
        int currentNumber = 1;

        foreach (var wine in sortedWines)
        {
            wine.WineNumber = currentNumber;
            _wineRepository.UpdateWine(wine);
            assignments[wine.WineId] = currentNumber;
            currentNumber++;
        }

        return Task.FromResult(assignments);
    }

    public int GetNextWineNumber(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.WineNumber.HasValue)
            .ToList();

        if (!wines.Any())
            return 1;

        return wines.Max(w => w.WineNumber!.Value) + 1;
    }

    public bool ValidateWineNumbers(string eventId)
    {
        var wines = _wineRepository.GetAllWines()
            .Where(w => w.EventId == eventId && w.WineNumber.HasValue)
            .ToList();

        if (!wines.Any())
            return true;

        // Check for duplicates
        var wineNumbers = wines.Select(w => w.WineNumber!.Value).ToList();
        return wineNumbers.Count == wineNumbers.Distinct().Count();
    }

    public List<WineCategory> GetCategoryOrder()
    {
        // Order per requirements: Hvit, Rosé, Dessert, Rød, Musserende, Hetvin
        return new List<WineCategory>
        {
            WineCategory.Hvitvin,
            WineCategory.Rosevin,
            WineCategory.Dessertvin,
            WineCategory.Rodvin,
            WineCategory.Mousserendevin,
            WineCategory.Hetvin
        };
    }
}

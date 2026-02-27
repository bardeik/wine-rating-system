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

    public async Task<Dictionary<string, int>> AssignWineNumbersAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.IsPaid)
            .ToList();

        if (!wines.Any())
            return new Dictionary<string, int>();

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
            await _wineRepository.UpdateWineAsync(wine);
            assignments[wine.WineId] = currentNumber;
            currentNumber++;
        }

        return assignments;
    }

    public async Task<int> GetNextWineNumberAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
            .Where(w => w.EventId == eventId && w.WineNumber.HasValue)
            .ToList();

        if (!wines.Any())
            return 1;

        return wines.Max(w => w.WineNumber!.Value) + 1;
    }

    public async Task<bool> ValidateWineNumbersAsync(string eventId)
    {
        var allWines = await _wineRepository.GetAllWinesAsync();
        var wines = allWines
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

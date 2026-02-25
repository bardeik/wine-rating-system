using WineApp.Models;

namespace WineApp.Services;

public interface IWineNumberService
{
    /// <summary>
    /// Assigns unique sequential wine numbers to all wines in an event
    /// Numbers are assigned by category order: Hvitvin, Rosevin, Dessertvin, Rodvin, Mousserendevin, Hetvin
    /// </summary>
    Task<Dictionary<string, int>> AssignWineNumbersAsync(string eventId);
    
    /// <summary>
    /// Gets the next available wine number for an event
    /// </summary>
    int GetNextWineNumber(string eventId);
    
    /// <summary>
    /// Validates that wine numbers are unique within an event
    /// </summary>
    bool ValidateWineNumbers(string eventId);
    
    /// <summary>
    /// Gets the category order for wine numbering
    /// </summary>
    List<WineCategory> GetCategoryOrder();
}

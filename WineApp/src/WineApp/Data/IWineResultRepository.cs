using WineApp.Models;

namespace WineApp.Data;

public interface IWineResultRepository
{
    Task<List<WineResult>> GetAllWineResultsAsync();
    Task<WineResult?> GetWineResultByIdAsync(string id);
    Task<WineResult?> GetWineResultByWineIdAsync(string wineId);
    Task<List<WineResult>> GetWineResultsByClassificationAsync(string classification);
    Task<List<WineResult>> GetOutlierWineResultsAsync();
    Task AddWineResultAsync(WineResult wineResult);
    Task UpdateWineResultAsync(WineResult wineResult);
    Task DeleteWineResultAsync(string id);
    Task DeleteWineResultByWineIdAsync(string wineId);
}

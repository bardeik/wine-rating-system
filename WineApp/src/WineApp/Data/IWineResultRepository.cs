using WineApp.Models;

namespace WineApp.Data;

public interface IWineResultRepository
{
    List<WineResult> GetAllWineResults();
    WineResult? GetWineResultById(string id);
    WineResult? GetWineResultByWineId(string wineId);
    List<WineResult> GetWineResultsByClassification(string classification);
    List<WineResult> GetOutlierWineResults();
    void AddWineResult(WineResult wineResult);
    void UpdateWineResult(WineResult wineResult);
    void DeleteWineResult(string id);
    void DeleteWineResultByWineId(string wineId);
}

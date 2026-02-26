using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineResultRepository : IWineResultRepository
{
    private readonly IMongoCollection<WineResult> _wineResults;

    public WineResultRepository(WineMongoDbContext context) =>
        _wineResults = context.WineResults;

    public List<WineResult> GetAllWineResults() => 
        _wineResults.Find(_ => true).ToList();

    public WineResult? GetWineResultById(string id) => 
        _wineResults.Find(wr => wr.WineResultId == id).FirstOrDefault();

    public WineResult? GetWineResultByWineId(string wineId) => 
        _wineResults.Find(wr => wr.WineId == wineId).FirstOrDefault();

    public List<WineResult> GetWineResultsByClassification(string classification) => 
        _wineResults.Find(wr => wr.Classification == classification).ToList();

    public List<WineResult> GetOutlierWineResults() => 
        _wineResults.Find(wr => wr.IsOutlier).ToList();

    public void AddWineResult(WineResult wineResult) => 
        _wineResults.InsertOne(wineResult);

    public void UpdateWineResult(WineResult wineResult) => 
        _wineResults.ReplaceOne(wr => wr.WineResultId == wineResult.WineResultId, wineResult);

    public void DeleteWineResult(string id) => 
        _wineResults.DeleteOne(wr => wr.WineResultId == id);

    public void DeleteWineResultByWineId(string wineId) => 
        _wineResults.DeleteOne(wr => wr.WineId == wineId);
}

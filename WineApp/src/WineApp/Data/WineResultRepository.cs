using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class WineResultRepository : IWineResultRepository
{
    private readonly IMongoCollection<WineResult> _wineResults;

    public WineResultRepository(WineMongoDbContext context) =>
        _wineResults = context.WineResults;

    public async Task<List<WineResult>> GetAllWineResultsAsync() =>
        await _wineResults.Find(_ => true).ToListAsync();

    public async Task<WineResult?> GetWineResultByIdAsync(string id) =>
        await _wineResults.Find(wr => wr.WineResultId == id).FirstOrDefaultAsync();

    public async Task<WineResult?> GetWineResultByWineIdAsync(string wineId) =>
        await _wineResults.Find(wr => wr.WineId == wineId).FirstOrDefaultAsync();

    public async Task<List<WineResult>> GetWineResultsByClassificationAsync(string classification) =>
        await _wineResults.Find(wr => wr.Classification == classification).ToListAsync();

    public async Task<List<WineResult>> GetOutlierWineResultsAsync() =>
        await _wineResults.Find(wr => wr.IsOutlier).ToListAsync();

    public async Task AddWineResultAsync(WineResult wineResult) =>
        await _wineResults.InsertOneAsync(wineResult);

    public async Task UpdateWineResultAsync(WineResult wineResult) =>
        await _wineResults.ReplaceOneAsync(wr => wr.WineResultId == wineResult.WineResultId, wineResult);

    public async Task DeleteWineResultAsync(string id) =>
        await _wineResults.DeleteOneAsync(wr => wr.WineResultId == id);

    public async Task DeleteWineResultByWineIdAsync(string wineId) =>
        await _wineResults.DeleteOneAsync(wr => wr.WineId == wineId);
}

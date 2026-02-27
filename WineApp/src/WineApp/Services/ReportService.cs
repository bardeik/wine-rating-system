using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class ReportService : IReportService
{
    private readonly IWineRepository _wineRepository;
    private readonly IWineRatingRepository _wineRatingRepository;
    private readonly IWineProducerRepository _wineProducerRepository;

    public ReportService(
        IWineRepository wineRepository,
        IWineRatingRepository wineRatingRepository,
        IWineProducerRepository wineProducerRepository)
    {
        _wineRepository = wineRepository;
        _wineRatingRepository = wineRatingRepository;
        _wineProducerRepository = wineProducerRepository;
    }

    public async Task<List<WineReportRow>> GetWineReportRowsAsync()
    {
        var wines     = await _wineRepository.GetAllWinesAsync();
        var ratings   = await _wineRatingRepository.GetAllWineRatingsAsync();
        var producers = await _wineProducerRepository.GetAllWineProducersAsync();

        return wines
            .Select(wine =>
            {
                var wineRatings  = ratings.Where(r => r.WineId == wine.WineId).ToList();
                var producerName = producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId)?.WineyardName ?? "–";

                decimal avgApp   = wineRatings.Any() ? wineRatings.Average(r => r.Appearance) : 0;
                decimal avgNose  = wineRatings.Any() ? wineRatings.Average(r => r.Nose)        : 0;
                decimal avgTaste = wineRatings.Any() ? wineRatings.Average(r => r.Taste)       : 0;

                return new WineReportRow
                {
                    RatingName    = wine.RatingName,
                    Name          = wine.Name,
                    Group         = wine.Group.ToString(),
                    Class         = wine.Class.ToString(),
                    Category      = wine.Category.ToString(),
                    ProducerName  = producerName,
                    AvgAppearance = (double)avgApp,
                    AvgNose       = (double)avgNose,
                    AvgTaste      = (double)avgTaste,
                    Total         = (double)(avgApp + avgNose + avgTaste),
                    RatingCount   = wineRatings.Count
                };
            })
            .OrderByDescending(r => r.Total)
            .ToList();
    }
}

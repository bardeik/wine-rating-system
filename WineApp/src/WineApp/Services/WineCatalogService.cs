using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class WineCatalogService : IWineCatalogService
{
    private readonly IWineRepository _wineRepository;
    private readonly IWineProducerRepository _wineProducerRepository;

    public WineCatalogService(IWineRepository wineRepository, IWineProducerRepository wineProducerRepository)
    {
        _wineRepository = wineRepository;
        _wineProducerRepository = wineProducerRepository;
    }

    public Task<IList<Wine>> GetAllWinesAsync() => _wineRepository.GetAllWinesAsync();
    public Task<Wine?> GetWineByIdAsync(string id) => _wineRepository.GetWineByIdAsync(id);
    public Task<IList<Wine>> GetWinesByIdsAsync(IEnumerable<string> wineIds) => _wineRepository.GetWinesByIdsAsync(wineIds);
    public Task AddWineAsync(Wine wine) => _wineRepository.AddWineAsync(wine);
    public Task UpdateWineAsync(Wine wine) => _wineRepository.UpdateWineAsync(wine);
    public Task DeleteWineAsync(string id) => _wineRepository.DeleteWineAsync(id);
    public Task<IList<WineProducer>> GetAllWineProducersAsync() => _wineProducerRepository.GetAllWineProducersAsync();
    public Task<WineProducer?> GetWineProducerByIdAsync(string id) => _wineProducerRepository.GetWineProducerByIdAsync(id);
    public Task<WineProducer?> GetWineProducerByUserIdAsync(string userId) => _wineProducerRepository.GetWineProducerByUserIdAsync(userId);
    public Task AddWineProducerAsync(WineProducer producer) => _wineProducerRepository.AddWineProducerAsync(producer);
    public Task UpdateWineProducerAsync(WineProducer producer) => _wineProducerRepository.UpdateWineProducerAsync(producer);
    public Task DeleteWineProducerAsync(string id) => _wineProducerRepository.DeleteWineProducerAsync(id);
}

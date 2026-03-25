using WineApp.Models;

namespace WineApp.Services;

public interface IWineCatalogService
{
    Task<IList<Wine>> GetAllWinesAsync();
    Task<Wine?> GetWineByIdAsync(string id);
    Task AddWineAsync(Wine wine);
    Task UpdateWineAsync(Wine wine);
    Task DeleteWineAsync(string id);
    Task<IList<WineProducer>> GetAllWineProducersAsync();
    Task<WineProducer?> GetWineProducerByIdAsync(string id);
    Task<WineProducer?> GetWineProducerByUserIdAsync(string userId);
    Task AddWineProducerAsync(WineProducer producer);
    Task UpdateWineProducerAsync(WineProducer producer);
    Task DeleteWineProducerAsync(string id);
}

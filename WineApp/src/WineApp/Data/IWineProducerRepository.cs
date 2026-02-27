using WineApp.Models;

namespace WineApp.Data;

public interface IWineProducerRepository
{
    Task<IList<WineProducer>> GetAllWineProducersAsync();
    Task<WineProducer?> GetWineProducerByIdAsync(string id);
    Task<string> AddWineProducerAsync(WineProducer wineProducer);
    Task UpdateWineProducerAsync(WineProducer wineProducer);
    Task DeleteWineProducerAsync(string id);
    Task<WineProducer?> GetWineProducerByUserIdAsync(string userId);
}

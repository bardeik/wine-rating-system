using WineApp.Models;

namespace WineApp.Data;

public interface IWineProducerRepository
{
    IList<WineProducer> GetAllWineProducers();
    WineProducer? GetWineProducerById(string id);
    string AddWineProducer(WineProducer wineProducer);
    void UpdateWineProducer(WineProducer wineProducer);
    void DeleteWineProducer(string id);
    WineProducer? GetWineProducerByUserId(string userId);
}

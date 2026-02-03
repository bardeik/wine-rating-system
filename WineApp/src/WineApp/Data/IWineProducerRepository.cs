using WineApp.Models;

namespace WineApp.Data;

public interface IWineProducerRepository
{
    IList<WineProducer> GetAllWineProducers();
    WineProducer? GetWineProducerById(int id);
    int AddWineProducer(WineProducer wineProducer);
    void DeleteWineProducer(int id);
}

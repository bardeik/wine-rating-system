using System.Collections.Generic;
using skeleton.Models;

namespace skeleton.Data
{
    public interface IWineProducerRepository
    {
        IList<WineProducer> GetAllWineProducers();
        WineProducer GetWineProducerById(int id);
        int AddWineProducer(WineProducer wineProducer);
        void DeleteWineProducer(int id);
    }
}

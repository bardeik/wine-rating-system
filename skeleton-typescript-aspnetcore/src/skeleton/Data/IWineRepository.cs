using System.Collections.Generic;
using skeleton.Models;

namespace skeleton.Data
{
    public interface IWineRepository
    {
        IList<Wine> GetAllWines();
        IList<Wine> GetAllWinesFromProducer(int producerId);
        Wine GetWineById(int id);
        int AddWine(Wine wine);
        void DeleteWine(int id);
    }
}

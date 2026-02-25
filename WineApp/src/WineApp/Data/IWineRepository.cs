using WineApp.Models;

namespace WineApp.Data;

public interface IWineRepository
{
    IList<Wine> GetAllWines();
    IList<Wine> GetAllWinesFromProducer(string producerId);
    Wine? GetWineById(string id);
    string AddWine(Wine wine);
    void UpdateWine(Wine wine);
    void DeleteWine(string id);
}

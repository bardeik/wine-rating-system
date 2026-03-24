using WineApp.Models;

namespace WineApp.Data;

public interface IWineRepository
{
    Task<IList<Wine>> GetAllWinesAsync();
    Task<IList<Wine>> GetAllWinesFromProducerAsync(string producerId);
    Task<IList<Wine>> GetWinesByIdsAsync(IEnumerable<string> wineIds);
    Task<Wine?> GetWineByIdAsync(string id);
    Task<string> AddWineAsync(Wine wine);
    Task UpdateWineAsync(Wine wine);
    Task DeleteWineAsync(string id);
}

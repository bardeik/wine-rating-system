using WineApp.Shared.Dtos;

namespace WineApp.Shared.MobileServices;

public interface IMobileWineService
{
    Task<IList<WineDto>> GetWinesForActiveEventAsync();
    Task<WineDto?> GetWineByIdAsync(string wineId);
}

using WineApp.Shared.Dtos;

namespace WineApp.Shared.MobileServices;

public interface IMobileFlightService
{
    Task<IList<WineDto>> GetMyFlightWinesAsync();
}

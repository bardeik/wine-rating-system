using WineApp.Models;

namespace WineApp.Services;

public interface IReportService
{
    Task<List<WineReportRow>> GetWineReportRowsAsync();
}

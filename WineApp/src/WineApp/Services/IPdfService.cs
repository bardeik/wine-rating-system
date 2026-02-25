using WineApp.Models;

namespace WineApp.Services;

public interface IPdfService
{
    /// <summary>
    /// Generates PDF report for trophy winners
    /// </summary>
    byte[] GenerateTrophyReport(
        Event eventData,
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers);
}

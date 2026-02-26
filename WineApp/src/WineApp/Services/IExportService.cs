using WineApp.Models;

namespace WineApp.Services;

public interface IExportService
{
    /// <summary>
    /// Exports results to CSV format
    /// </summary>
    string ExportResultsToCSV(List<WineResult> results, List<Wine> wines, List<WineProducer> producers);

    /// <summary>
    /// Exports trophy winners to CSV format
    /// </summary>
    string ExportTrophiesToCSV(
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers);

    /// <summary>
    /// Exports complete event data (wines, ratings, results)
    /// </summary>
    string ExportEventData(
        Event eventData,
        List<Wine> wines,
        List<WineRating> ratings,
        List<WineResult> results,
        List<WineProducer> producers);

    /// <summary>
    /// Exports flight lists for printing
    /// </summary>
    string ExportFlightList(List<Flight> flights, List<Wine> wines);

    /// <summary>
    /// Returns UTF-8 BOM bytes for Excel-compatible CSV download
    /// </summary>
    byte[] GetCSVBytes(string csvContent);
}

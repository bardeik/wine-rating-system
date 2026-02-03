using WineApp.Models;

namespace WineApp.Data;

public class WineProducerRepository : IWineProducerRepository
{
    private readonly List<WineProducer> wineProducers = new()
    {
        new WineProducer
        {
            WineProducerId = 1,
            Address = "Test adresse 21",
            City= "Oslo",
            Country = "Norway",
            Email= "bestWines@fluffy.com",
            OrganisationNumber="111122223333445",
            ResponsibleProducerName="Test Testersen",
            WineyardName="Oslo Vest Wines AS",
            Zip="0125"
        },
        new WineProducer
        {
            WineProducerId = 2,
            Address = "Test adresse Ny 15",
            City= "Grimstad",
            Country = "Norway",
            Email= "bestWinesEver@fluffier.com",
            OrganisationNumber="111122234567890",
            ResponsibleProducerName="Petter Testeren",
            WineyardName="Grimstad Vin og Vann AS",
            Zip="4525"
        },
        new WineProducer
        {
            WineProducerId = 3,
            Address = "Agder Alle 21",
            City= "Kristiansand",
            Country = "Norway",
            Email= "bardeh@gmail.com",
            OrganisationNumber="222222223333445",
            ResponsibleProducerName="Bård Eik-Hvidsten",
            WineyardName="Tech Wine AS",
            Zip="4631"
        }
    };
    
    public IList<WineProducer> GetAllWineProducers() => wineProducers;
    
    public WineProducer? GetWineProducerById(int id) => 
        wineProducers.Find(wineProducer => wineProducer.WineProducerId == id);

    public int AddWineProducer(WineProducer wineProducer)
    {
        var newId = wineProducers.Count > 0 ? wineProducers.Max(x => x.WineProducerId) + 1 : 1;
        wineProducer.WineProducerId = newId;
        wineProducers.Add(wineProducer);
        return newId;
    }

    public void DeleteWineProducer(int id)
    {
        var producer = wineProducers.SingleOrDefault(x => x.WineProducerId == id);
        if (producer != null)
            wineProducers.Remove(producer);
    }
}

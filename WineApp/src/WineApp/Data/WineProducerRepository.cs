using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WineApp.Models;

namespace WineApp.Data
{
    public class WineProducerRepository : IWineProducerRepository
    {
        private List<WineProducer> wineProducers = new List<WineProducer>()
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
        public IList<WineProducer> GetAllWineProducers()
        {
            return wineProducers;
        }
        public WineProducer GetWineProducerById(int id)
        {
            return wineProducers.Find(wineProducer => wineProducer.WineProducerId == id);
        }

        public int AddWineProducer(WineProducer wineProducer)
        {
            var newId = wineProducers.Max(x => x.WineProducerId) + 1;
            wineProducer.WineProducerId = newId;
            wineProducers.Add(wineProducer);
            return newId;
        }

        public void DeleteWineProducer(int id)
        {
            wineProducers.Remove(wineProducers.Single(x => x.WineProducerId == id));
        }

    }
}

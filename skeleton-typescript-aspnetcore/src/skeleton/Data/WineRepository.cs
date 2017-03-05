using System.Collections.Generic;
using System.Linq;
using skeleton.Models;

namespace skeleton.Data
{
    public class WineRepository : IWineRepository
    {
        private List<Wine> wines = new List<Wine>()
        {
            new Wine {
                WineId = 1,
                Name = "Polets røde",
                RatingName="Hemmelig Polets Røde",
                WineProducerId = 1,
                Category = WineCategory.Rodvin,
                Class = WineClass.Eldre,
                Group= WineGroup.A
            },
            new Wine {
                WineId = 2,
                Name = "Polets andre røde",
                RatingName="Hemmelig Andre Polets Røde",
                WineProducerId = 1,
                Category = WineCategory.Rodvin,
                Class = WineClass.Unge,
                Group= WineGroup.C
            },
            new Wine {
                WineId = 3,
                Name = "Polets røde",
                RatingName="Hemmelig Tredje Polets Røde",
                WineProducerId = 2,
                Category = WineCategory.Rodvin,
                Class = WineClass.Unge,
                Group= WineGroup.B
            },

        };
        
        public IList<Wine> GetAllWines()
        {
            return wines;
        }

        public Wine GetWineById(int id)
        {
            return wines.Find(wine => wine.WineId == id);
        }

        public IList<Wine> GetAllWinesFromProducer(int producerId)
        {
            return wines.FindAll(wine => wine.WineProducerId == producerId);
        }

        public int AddWine(Wine wine)
        {
            var newId = wines.Max(x => x.WineId) + 1;
            wine.WineId = newId;
            wines.Add(wine);
            return newId;
        }

        public void DeleteWine(int id)
        {
            wines.Remove(wines.Single(x => x.WineId == id));
        }
    }
}

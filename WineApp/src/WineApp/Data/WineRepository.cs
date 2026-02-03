using WineApp.Models;

namespace WineApp.Data;

public class WineRepository : IWineRepository
{
    private readonly List<Wine> wines = new()
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
    
    public IList<Wine> GetAllWines() => wines;

    public Wine? GetWineById(int id) => wines.Find(wine => wine.WineId == id);

    public IList<Wine> GetAllWinesFromProducer(int producerId) => 
        wines.FindAll(wine => wine.WineProducerId == producerId);

    public int AddWine(Wine wine)
    {
        var newId = wines.Count > 0 ? wines.Max(x => x.WineId) + 1 : 1;
        wine.WineId = newId;
        wines.Add(wine);
        return newId;
    }

    public void DeleteWine(int id)
    {
        var wine = wines.SingleOrDefault(x => x.WineId == id);
        if (wine != null)
            wines.Remove(wine);
    }
}

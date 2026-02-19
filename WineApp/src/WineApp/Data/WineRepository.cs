using Microsoft.EntityFrameworkCore;
using WineApp.Models;

namespace WineApp.Data;

public class WineRepository : IWineRepository
{
    private readonly IDbContextFactory<WineAppDbContext> _contextFactory;

    public WineRepository(IDbContextFactory<WineAppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public IList<Wine> GetAllWines()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Wines.ToList();
    }

    public Wine? GetWineById(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Wines.Find(id);
    }

    public IList<Wine> GetAllWinesFromProducer(int producerId)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Wines.Where(w => w.WineProducerId == producerId).ToList();
    }

    public int AddWine(Wine wine)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Wines.Add(wine);
        context.SaveChanges();
        return wine.WineId;
    }

    public void DeleteWine(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var wine = context.Wines.Find(id);
        if (wine != null)
        {
            context.Wines.Remove(wine);
            context.SaveChanges();
        }
    }
}

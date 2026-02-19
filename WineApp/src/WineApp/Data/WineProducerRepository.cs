using Microsoft.EntityFrameworkCore;
using WineApp.Models;

namespace WineApp.Data;

public class WineProducerRepository : IWineProducerRepository
{
    private readonly IDbContextFactory<WineAppDbContext> _contextFactory;

    public WineProducerRepository(IDbContextFactory<WineAppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public IList<WineProducer> GetAllWineProducers()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.WineProducers.ToList();
    }

    public WineProducer? GetWineProducerById(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.WineProducers.Find(id);
    }

    public int AddWineProducer(WineProducer wineProducer)
    {
        using var context = _contextFactory.CreateDbContext();
        context.WineProducers.Add(wineProducer);
        context.SaveChanges();
        return wineProducer.WineProducerId;
    }

    public void DeleteWineProducer(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var producer = context.WineProducers.Find(id);
        if (producer != null)
        {
            context.WineProducers.Remove(producer);
            context.SaveChanges();
        }
    }
}

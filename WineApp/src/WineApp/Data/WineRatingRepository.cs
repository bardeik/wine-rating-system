using Microsoft.EntityFrameworkCore;
using WineApp.Models;

namespace WineApp.Data;

public class WineRatingRepository : IWineRatingRepository
{
    private readonly IDbContextFactory<WineAppDbContext> _contextFactory;

    public WineRatingRepository(IDbContextFactory<WineAppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public IList<WineRating> GetAllWineRatings()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.WineRatings.ToList();
    }

    public WineRating? GetWineRatingById(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.WineRatings.Find(id);
    }

    public int AddWineRating(WineRating wineRating)
    {
        using var context = _contextFactory.CreateDbContext();
        context.WineRatings.Add(wineRating);
        context.SaveChanges();
        return wineRating.WineRatingId;
    }

    public void DeleteWineRating(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var rating = context.WineRatings.Find(id);
        if (rating != null)
        {
            context.WineRatings.Remove(rating);
            context.SaveChanges();
        }
    }
}

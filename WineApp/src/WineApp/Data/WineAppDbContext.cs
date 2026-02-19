using Microsoft.EntityFrameworkCore;
using WineApp.Models;

namespace WineApp.Data;

public class WineAppDbContext : DbContext
{
    public WineAppDbContext(DbContextOptions<WineAppDbContext> options) : base(options) { }

    public DbSet<WineProducer> WineProducers => Set<WineProducer>();
    public DbSet<Wine> Wines => Set<Wine>();
    public DbSet<WineRating> WineRatings => Set<WineRating>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WineProducer>().HasData(
            new WineProducer
            {
                WineProducerId = 1,
                Address = "Test adresse 21",
                City = "Oslo",
                Country = "Norway",
                Email = "bestWines@fluffy.com",
                OrganisationNumber = "111122223333445",
                ResponsibleProducerName = "Test Testersen",
                WineyardName = "Oslo Vest Wines AS",
                Zip = "0125"
            },
            new WineProducer
            {
                WineProducerId = 2,
                Address = "Test adresse Ny 15",
                City = "Grimstad",
                Country = "Norway",
                Email = "bestWinesEver@fluffier.com",
                OrganisationNumber = "111122234567890",
                ResponsibleProducerName = "Petter Testeren",
                WineyardName = "Grimstad Vin og Vann AS",
                Zip = "4525"
            },
            new WineProducer
            {
                WineProducerId = 3,
                Address = "Agder Alle 21",
                City = "Kristiansand",
                Country = "Norway",
                Email = "bardeh@gmail.com",
                OrganisationNumber = "222222223333445",
                ResponsibleProducerName = "Bård Eik",
                WineyardName = "Tech Wine AS",
                Zip = "4631"
            }
        );

        modelBuilder.Entity<Wine>().HasData(
            new Wine
            {
                WineId = 1,
                Name = "Polets røde",
                RatingName = "Hemmelig Polets Røde",
                WineProducerId = 1,
                Category = WineCategory.Rodvin,
                Class = WineClass.Eldre,
                Group = WineGroup.A
            },
            new Wine
            {
                WineId = 2,
                Name = "Polets andre røde",
                RatingName = "Hemmelig Andre Polets Røde",
                WineProducerId = 1,
                Category = WineCategory.Rodvin,
                Class = WineClass.Unge,
                Group = WineGroup.C
            },
            new Wine
            {
                WineId = 3,
                Name = "Polets røde",
                RatingName = "Hemmelig Tredje Polets Røde",
                WineProducerId = 2,
                Category = WineCategory.Rodvin,
                Class = WineClass.Unge,
                Group = WineGroup.B
            }
        );

        modelBuilder.Entity<WineRating>().HasData(
            new WineRating { WineRatingId = 1, JudgeId = "Hans", Nose = 4, Taste = 5, Visuality = 5, WineId = 1 },
            new WineRating { WineRatingId = 2, JudgeId = "Petter", Nose = 3, Taste = 4, Visuality = 3, WineId = 1 },
            new WineRating { WineRatingId = 3, JudgeId = "Frans", Nose = 5, Taste = 4, Visuality = 6, WineId = 1 },
            new WineRating { WineRatingId = 4, JudgeId = "Ola", Nose = 5, Taste = 4, Visuality = 4, WineId = 1 }
        );

        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem { Id = 1, Name = "Prepare wine samples for tasting", CreatedAt = new DateTime(2024, 01, 15), DueDate = new DateTime(2024, 02, 01) },
            new TodoItem { Id = 2, Name = "Review judge assignments", CreatedAt = new DateTime(2024, 01, 20), DueDate = new DateTime(2024, 01, 31) },
            new TodoItem { Id = 3, Name = "Compile final wine ratings report", CreatedAt = new DateTime(2024, 02, 05), DueDate = new DateTime(2024, 02, 15) }
        );
    }
}

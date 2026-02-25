using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using WineApp.Data;
using WineApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// MongoDB context (domain collections)
builder.Services.AddSingleton<WineMongoDbContext>();

// MongoDB Identity
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "wineapp";

builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
})
.AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(mongoConnectionString, mongoDatabaseName)
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Register repositories
builder.Services.AddSingleton<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddSingleton<IWineRepository, WineRepository>();

var app = builder.Build();

// Seed roles, default admin and sample data on startup
using (var scope = app.Services.CreateScope())
{
    await SeedAsync(scope.ServiceProvider);
}

async Task SeedAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<MongoIdentityRole<Guid>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "Judge", "Viewer", "WineProducer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new MongoIdentityRole<Guid>(role));
    }

    if (await userManager.FindByEmailAsync("admin@wineapp.com") is null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@wineapp.com",
            Email = "admin@wineapp.com",
            DisplayName = "Administrator",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed sample data if collections are empty
    var wineProducerRepo = services.GetRequiredService<IWineProducerRepository>();
    var wineRepo = services.GetRequiredService<IWineRepository>();
    var wineRatingRepo = services.GetRequiredService<IWineRatingRepository>();

    // Seed judge users if none exist with the Judge role
    var judgeUsers = await userManager.GetUsersInRoleAsync("Judge");
    if (judgeUsers.Count == 0)
    {
        foreach (var name in new[] { "Frans", "Hans", "Ola", "Petter" })
        {
            var email = $"{name.ToLower()}@wineapp.com";
            var judgeUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = name,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(judgeUser, "Judge123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(judgeUser, "Judge");
        }
    }

    if (wineProducerRepo.GetAllWineProducers().Count == 0)
    {
        var p1 = new WineProducer { WineProducerId = ObjectId.GenerateNewId().ToString(), Address = "Test adresse 21", City = "Oslo", Country = "Norway", Email = "bestWines@fluffy.com", OrganisationNumber = "111122223333445", ResponsibleProducerName = "Test Testersen", WineyardName = "Oslo Vest Wines AS", Zip = "0125" };
        var p2 = new WineProducer { WineProducerId = ObjectId.GenerateNewId().ToString(), Address = "Test adresse Ny 15", City = "Grimstad", Country = "Norway", Email = "bestWinesEver@fluffier.com", OrganisationNumber = "111122234567890", ResponsibleProducerName = "Petter Testeren", WineyardName = "Grimstad Vin og Vann AS", Zip = "4525" };
        var p3 = new WineProducer { WineProducerId = ObjectId.GenerateNewId().ToString(), Address = "Agder Alle 21", City = "Kristiansand", Country = "Norway", Email = "bardeh@gmail.com", OrganisationNumber = "222222223333445", ResponsibleProducerName = "Bård Eik", WineyardName = "Tech Wine AS", Zip = "4631" };
        wineProducerRepo.AddWineProducer(p1);
        wineProducerRepo.AddWineProducer(p2);
        wineProducerRepo.AddWineProducer(p3);

        if (wineRepo.GetAllWines().Count == 0)
        {
            var w1 = new Wine { WineId = ObjectId.GenerateNewId().ToString(), Name = "Polets røde", RatingName = "Hemmelig Polets Røde", WineProducerId = p1.WineProducerId, Category = WineCategory.Rodvin, Class = WineClass.Eldre, Group = WineGroup.A };
            var w2 = new Wine { WineId = ObjectId.GenerateNewId().ToString(), Name = "Polets andre røde", RatingName = "Hemmelig Andre Polets Røde", WineProducerId = p1.WineProducerId, Category = WineCategory.Rodvin, Class = WineClass.Unge, Group = WineGroup.C };
            var w3 = new Wine { WineId = ObjectId.GenerateNewId().ToString(), Name = "Polets røde", RatingName = "Hemmelig Tredje Polets Røde", WineProducerId = p2.WineProducerId, Category = WineCategory.Rodvin, Class = WineClass.Unge, Group = WineGroup.B };
            wineRepo.AddWine(w1);
            wineRepo.AddWine(w2);
            wineRepo.AddWine(w3);

            if (wineRatingRepo.GetAllWineRatings().Count == 0)
            {
                wineRatingRepo.AddWineRating(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Hans",   Nose = 4, Taste = 5, Visuality = 5, WineId = w1.WineId });
                wineRatingRepo.AddWineRating(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Petter", Nose = 3, Taste = 4, Visuality = 3, WineId = w1.WineId });
                wineRatingRepo.AddWineRating(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Frans",  Nose = 5, Taste = 4, Visuality = 6, WineId = w1.WineId });
                wineRatingRepo.AddWineRating(new WineRating { WineRatingId = ObjectId.GenerateNewId().ToString(), JudgeId = "Ola",    Nose = 5, Taste = 4, Visuality = 4, WineId = w1.WineId });
            }
        }
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
